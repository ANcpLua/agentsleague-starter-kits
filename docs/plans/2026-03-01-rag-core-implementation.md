# RAG.Core Implementation Plan

> **For Claude:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task.

**Goal:** Build a reusable hybrid search RAG pipeline (RAG.Core) and integrate it into Track 3's KnowledgeAgent, replacing naive keyword matching with Azure AI Search vector + BM25 + semantic reranking.

**Architecture:** Shared `RAG.Core` class library consumed by `EnterpriseSupportAgent`. Separate `RAG.Indexer` CLI tool for indexing local markdown or web docs. KnowledgeAgent's `HandleKnowledgeQueryAsync` calls `IHybridSearchService.SearchAsync()` instead of `FindRelevantDocuments()`.

**Tech Stack:** Azure.Search.Documents 11.7.0, Azure.AI.OpenAI (already in Track 3), AngleSharp 1.4.0, net10.0

**Design doc:** `docs/plans/2026-03-01-rag-core-design.md`

---

### Task 1: Create RAG.Core project with models

**Files:**
- Create: `shared/RAG.Core/RAG.Core.csproj`
- Create: `shared/RAG.Core/Models/SearchDocument.cs`
- Create: `shared/RAG.Core/Models/SearchResult.cs`
- Create: `shared/RAG.Core/Models/ChunkMetadata.cs`
- Create: `shared/RAG.Core/Models/RagJsonContext.cs`
- Modify: `track-3-enterprise-agents.slnx` — add RAG.Core project reference

**Step 1: Create the csproj**

```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
    <RootNamespace>RAG.Core</RootNamespace>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Azure.Search.Documents" Version="11.7.0" />
    <PackageReference Include="Azure.AI.OpenAI" Version="2.2.0-beta.4" />
    <PackageReference Include="AngleSharp" Version="1.4.0" />
  </ItemGroup>

</Project>
```

Note: `Azure.AI.OpenAI` version must match what Track 3 already uses. Check `EnterpriseSupportAgent.csproj`'s resolved version via `dotnet list package` before finalizing. If Track 3 uses a different version, match it.

**Step 2: Create SearchDocument.cs (index schema)**

```csharp
using Azure.Search.Documents.Indexes;
using Azure.Search.Documents.Indexes.Models;

namespace RAG.Core.Models;

public sealed class SearchDocument
{
    [SimpleField(IsKey = true, IsFilterable = true)]
    public required string Id { get; init; }

    [SearchableField]
    public required string Content { get; init; }

    [SearchableField(IsFilterable = true)]
    public required string Title { get; init; }

    [SimpleField(IsFilterable = true, IsFacetable = true)]
    public required string Source { get; init; }

    [SimpleField(IsFilterable = true, IsFacetable = true)]
    public required string Category { get; init; }

    [SearchableField]
    public required string HeadingChain { get; init; }

    [VectorSearchField(
        VectorSearchDimensions = 1536,
        VectorSearchProfileName = "default-vector")]
    public ReadOnlyMemory<float>? ContentVector { get; init; }
}
```

**Step 3: Create SearchResult.cs (query result)**

```csharp
namespace RAG.Core.Models;

public sealed record SearchResult(
    string Content,
    string Title,
    string Source,
    string HeadingChain,
    double Score);
```

**Step 4: Create ChunkMetadata.cs**

```csharp
namespace RAG.Core.Models;

public sealed record ChunkMetadata(
    string Title,
    string Source,
    string Category,
    string HeadingChain,
    int ChunkIndex);
```

**Step 5: Create RagJsonContext.cs (AOT-safe serialization)**

```csharp
using System.Text.Json.Serialization;

namespace RAG.Core.Models;

[JsonSerializable(typeof(SearchResult))]
[JsonSerializable(typeof(IReadOnlyList<SearchResult>))]
[JsonSerializable(typeof(ChunkMetadata))]
[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
internal sealed partial class RagJsonContext : JsonSerializerContext;
```

**Step 6: Add RAG.Core to the Track 3 solution**

Update `track-3-enterprise-agents.slnx`:

```xml
<Solution>
    <Folder Name="/docs/">
        <File Path="track-3-enterprise-agents/README.md"/>
        <File Path="README.md"/>
        <File Path="FILE-MAP.md"/>
        <File Path="CLAUDE.md"/>
    </Folder>
    <Folder Name="/shared/">
        <File Path="docs/plans/2026-03-01-rag-core-design.md"/>
    </Folder>
    <Project Path="shared/RAG.Core/RAG.Core.csproj"/>
    <Project Path="track-3-enterprise-agents/src/EnterpriseSupportAgent/EnterpriseSupportAgent.csproj"/>
    <Project Path="track-3-enterprise-agents/src/SupportMcpServer/SupportMcpServer.csproj"/>
</Solution>
```

**Step 7: Verify it builds**

Run: `dotnet build shared/RAG.Core/RAG.Core.csproj`
Expected: Build succeeded, 0 errors

**Step 8: Commit**

```bash
git add shared/RAG.Core/ track-3-enterprise-agents.slnx
git commit -m "feat(rag): scaffold RAG.Core project with index models"
```

---

### Task 2: Implement MarkdownChunker

**Files:**
- Create: `shared/RAG.Core/Chunking/MarkdownChunker.cs`

**Step 1: Implement heading-aware chunker**

```csharp
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using RAG.Core.Models;

namespace RAG.Core.Chunking;

public static partial class MarkdownChunker
{
    private const int DefaultMaxChunkSize = 1500;

    public static IReadOnlyList<(string Content, ChunkMetadata Metadata)> Chunk(
        string markdown,
        string source,
        string? category = null,
        int maxChunkSize = DefaultMaxChunkSize)
    {
        // 1. Strip YAML frontmatter, extract metadata
        var (body, frontmatter) = StripFrontmatter(markdown);
        category ??= frontmatter.GetValueOrDefault("track") ?? InferCategory(source);
        var pageTitle = frontmatter.GetValueOrDefault("name") ?? ExtractFirstHeading(body) ?? source;

        // 2. Split into sections by heading
        var sections = SplitByHeadings(body);

        // 3. Build chunks with heading chain
        var chunks = new List<(string Content, ChunkMetadata Metadata)>();
        var headingStack = new List<(int Level, string Text)>();
        var chunkIndex = 0;

        foreach (var section in sections)
        {
            // Update heading stack
            if (section.HeadingLevel > 0)
            {
                while (headingStack.Count > 0 &&
                       headingStack[^1].Level >= section.HeadingLevel)
                    headingStack.RemoveAt(headingStack.Count - 1);
                headingStack.Add((section.HeadingLevel, section.HeadingText));
            }

            var headingChain = string.Join(" > ", headingStack.Select(h => h.Text));
            var content = section.Body.Trim();

            if (string.IsNullOrWhiteSpace(content))
                continue;

            // 4. Split oversized chunks on paragraph boundaries
            if (content.Length > maxChunkSize)
            {
                foreach (var subChunk in SplitOnParagraphs(content, maxChunkSize))
                {
                    chunks.Add((subChunk, new ChunkMetadata(
                        Title: headingStack.Count > 0 ? headingStack[^1].Text : pageTitle,
                        Source: source,
                        Category: category,
                        HeadingChain: headingChain,
                        ChunkIndex: chunkIndex++)));
                }
            }
            else
            {
                chunks.Add((content, new ChunkMetadata(
                    Title: headingStack.Count > 0 ? headingStack[^1].Text : pageTitle,
                    Source: source,
                    Category: category,
                    HeadingChain: headingChain,
                    ChunkIndex: chunkIndex++)));
            }
        }

        return chunks;
    }

    public static string GenerateChunkId(string source, int chunkIndex)
    {
        var input = $"{source}:{chunkIndex}";
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(input));
        return Convert.ToHexStringLower(hash)[..16];
    }

    private static (string Body, Dictionary<string, string> Frontmatter) StripFrontmatter(string markdown)
    {
        if (!markdown.StartsWith("---"))
            return (markdown, new Dictionary<string, string>());

        var endIndex = markdown.IndexOf("\n---", 3, StringComparison.Ordinal);
        if (endIndex < 0)
            return (markdown, new Dictionary<string, string>());

        var frontmatterBlock = markdown[3..endIndex].Trim();
        var body = markdown[(endIndex + 4)..].TrimStart();

        var metadata = new Dictionary<string, string>();
        foreach (var line in frontmatterBlock.Split('\n'))
        {
            var colonIndex = line.IndexOf(':');
            if (colonIndex <= 0) continue;
            var key = line[..colonIndex].Trim();
            var value = line[(colonIndex + 1)..].Trim().Trim('"');
            if (!string.IsNullOrEmpty(value))
                metadata[key] = value;
        }

        return (body, metadata);
    }

    private static List<SectionInfo> SplitByHeadings(string markdown)
    {
        var sections = new List<SectionInfo>();
        var lines = markdown.Split('\n');
        var currentBody = new StringBuilder();
        var currentLevel = 0;
        var currentHeading = "";
        var inCodeBlock = false;

        foreach (var line in lines)
        {
            // Track code fences
            if (line.TrimStart().StartsWith("```"))
            {
                inCodeBlock = !inCodeBlock;
                currentBody.AppendLine(line);
                continue;
            }

            if (inCodeBlock)
            {
                currentBody.AppendLine(line);
                continue;
            }

            var headingMatch = HeadingPattern().Match(line);
            if (headingMatch.Success)
            {
                // Flush previous section
                if (currentBody.Length > 0 || currentLevel > 0)
                {
                    sections.Add(new SectionInfo(currentLevel, currentHeading, currentBody.ToString()));
                    currentBody.Clear();
                }

                currentLevel = headingMatch.Groups[1].Value.Length;
                currentHeading = headingMatch.Groups[2].Value.Trim();
            }
            else
            {
                currentBody.AppendLine(line);
            }
        }

        // Flush last section
        if (currentBody.Length > 0 || currentLevel > 0)
            sections.Add(new SectionInfo(currentLevel, currentHeading, currentBody.ToString()));

        return sections;
    }

    private static IEnumerable<string> SplitOnParagraphs(string content, int maxSize)
    {
        var paragraphs = content.Split(["\n\n", "\r\n\r\n"], StringSplitOptions.RemoveEmptyEntries);
        var current = new StringBuilder();

        foreach (var paragraph in paragraphs)
        {
            if (current.Length + paragraph.Length > maxSize && current.Length > 0)
            {
                yield return current.ToString().Trim();
                current.Clear();
            }
            if (current.Length > 0) current.Append("\n\n");
            current.Append(paragraph);
        }

        if (current.Length > 0)
            yield return current.ToString().Trim();
    }

    private static string? ExtractFirstHeading(string markdown)
    {
        var match = HeadingPattern().Match(markdown);
        return match.Success ? match.Groups[2].Value.Trim() : null;
    }

    private static string InferCategory(string source)
    {
        // Extract directory name as category: "Knowledge/it-solution/it-solution.md" → "it-solution"
        var parts = source.Replace('\\', '/').Split('/');
        return parts.Length >= 2 ? parts[^2] : "general";
    }

    [GeneratedRegex(@"^(#{1,6})\s+(.+)$")]
    private static partial Regex HeadingPattern();

    private sealed record SectionInfo(int HeadingLevel, string HeadingText, string Body);
}
```

**Step 2: Verify it builds**

Run: `dotnet build shared/RAG.Core/RAG.Core.csproj`
Expected: Build succeeded

**Step 3: Commit**

```bash
git add shared/RAG.Core/Chunking/
git commit -m "feat(rag): implement heading-aware markdown chunker"
```

---

### Task 3: Implement AzureOpenAIEmbedder

**Files:**
- Create: `shared/RAG.Core/Embedding/IEmbedder.cs`
- Create: `shared/RAG.Core/Embedding/AzureOpenAIEmbedder.cs`
- Create: `shared/RAG.Core/Configuration/RagOptions.cs`

**Step 1: Create IEmbedder interface**

```csharp
namespace RAG.Core.Embedding;

public interface IEmbedder
{
    Task<ReadOnlyMemory<float>> EmbedAsync(string text, CancellationToken ct = default);
    Task<IReadOnlyList<ReadOnlyMemory<float>>> EmbedBatchAsync(
        IReadOnlyList<string> texts, CancellationToken ct = default);
}
```

**Step 2: Create RagOptions**

```csharp
namespace RAG.Core.Configuration;

public sealed class RagOptions
{
    public required string SearchEndpoint { get; init; }
    public string? SearchApiKey { get; init; }
    public required string IndexName { get; init; }
    public string EmbeddingEndpoint { get; init; } = "https://models.inference.ai.azure.com";
    public string? EmbeddingApiKey { get; init; }
    public string EmbeddingModel { get; init; } = "text-embedding-3-small";
    public bool UseSemanticRanker { get; init; } = true;
    public int MaxChunkSize { get; init; } = 1500;
    public int MaxCrawlPages { get; init; } = 100;
}
```

**Step 3: Implement AzureOpenAIEmbedder**

```csharp
using System.ClientModel;
using OpenAI;
using OpenAI.Embeddings;

namespace RAG.Core.Embedding;

public sealed class AzureOpenAIEmbedder : IEmbedder
{
    private readonly EmbeddingClient _client;

    public AzureOpenAIEmbedder(string endpoint, string apiKey, string model)
    {
        var clientOptions = new OpenAIClientOptions { Endpoint = new Uri(endpoint) };
        var openAiClient = new OpenAIClient(new ApiKeyCredential(apiKey), clientOptions);
        _client = openAiClient.GetEmbeddingClient(model);
    }

    public async Task<ReadOnlyMemory<float>> EmbedAsync(string text, CancellationToken ct = default)
    {
        var result = await _client.GenerateEmbeddingAsync(text, cancellationToken: ct);
        return result.Value.ToFloats();
    }

    public async Task<IReadOnlyMemory<float>>> EmbedBatchAsync(
        IReadOnlyList<string> texts, CancellationToken ct = default)
    {
        var result = await _client.GenerateEmbeddingsAsync(texts, cancellationToken: ct);
        return result.Value.Select(e => e.ToFloats()).ToList();
    }
}
```

**Step 4: Verify it builds**

Run: `dotnet build shared/RAG.Core/RAG.Core.csproj`
Expected: Build succeeded

**Step 5: Commit**

```bash
git add shared/RAG.Core/Embedding/ shared/RAG.Core/Configuration/
git commit -m "feat(rag): implement Azure OpenAI embedding service"
```

---

### Task 4: Implement HybridSearchService

**Files:**
- Create: `shared/RAG.Core/Search/IHybridSearchService.cs`
- Create: `shared/RAG.Core/Search/HybridSearchService.cs`
- Create: `shared/RAG.Core/Search/RagSearchOptions.cs`

**Step 1: Create interfaces**

```csharp
using RAG.Core.Models;

namespace RAG.Core.Search;

public interface IHybridSearchService
{
    Task<IReadOnlyList<SearchResult>> SearchAsync(
        string query,
        RagSearchOptions? options = null,
        CancellationToken ct = default);
}

public sealed record RagSearchOptions(
    string? CategoryFilter = null,
    int TopK = 5,
    int CandidateCount = 50,
    bool UseSemanticRanker = true);
```

**Step 2: Implement HybridSearchService**

```csharp
using Azure;
using Azure.Search.Documents;
using Azure.Search.Documents.Models;
using RAG.Core.Configuration;
using RAG.Core.Embedding;
using RAG.Core.Models;

namespace RAG.Core.Search;

public sealed class HybridSearchService : IHybridSearchService
{
    private readonly SearchClient _searchClient;
    private readonly IEmbedder _embedder;
    private readonly RagOptions _options;

    public HybridSearchService(SearchClient searchClient, IEmbedder embedder, RagOptions options)
    {
        _searchClient = searchClient;
        _embedder = embedder;
        _options = options;
    }

    public async Task<IReadOnlyList<SearchResult>> SearchAsync(
        string query,
        RagSearchOptions? options = null,
        CancellationToken ct = default)
    {
        options ??= new RagSearchOptions(UseSemanticRanker: _options.UseSemanticRanker);

        // 1. Embed the query for vector search
        var queryVector = await _embedder.EmbedAsync(query, ct);

        // 2. Build hybrid search request (vector + BM25)
        var searchOptions = new Azure.Search.Documents.SearchOptions
        {
            Size = options.TopK,
            Select = { "content", "title", "source", "headingChain" },
            VectorSearch = new()
            {
                Queries =
                {
                    new VectorizedQuery(queryVector)
                    {
                        KNearestNeighborsCount = options.CandidateCount,
                        Fields = { "contentVector" }
                    }
                }
            },
            QueryType = options.UseSemanticRanker
                ? Azure.Search.Documents.Models.SearchQueryType.Semantic
                : Azure.Search.Documents.Models.SearchQueryType.Simple,
        };

        // 3. Apply semantic configuration if using ranker
        if (options.UseSemanticRanker)
        {
            searchOptions.SemanticSearch = new SemanticSearchOptions
            {
                SemanticConfigurationName = "default-semantic",
                QueryCaption = new QueryCaption(QueryCaptionType.Extractive),
                QueryAnswer = new QueryAnswer(QueryAnswerType.Extractive)
            };
        }

        // 4. Apply category filter
        if (!string.IsNullOrEmpty(options.CategoryFilter))
        {
            searchOptions.Filter = $"category eq '{options.CategoryFilter}'";
        }

        // 5. Execute hybrid search
        var response = await _searchClient.SearchAsync<Models.SearchDocument>(
            query, searchOptions, ct);

        // 6. Map to SearchResult
        var results = new List<SearchResult>();
        await foreach (var result in response.Value.GetResultsAsync())
        {
            results.Add(new SearchResult(
                Content: result.Document.Content,
                Title: result.Document.Title,
                Source: result.Document.Source,
                HeadingChain: result.Document.HeadingChain,
                Score: result.Score ?? 0));
        }

        return results;
    }
}
```

**Step 3: Verify it builds**

Run: `dotnet build shared/RAG.Core/RAG.Core.csproj`
Expected: Build succeeded

**Step 4: Commit**

```bash
git add shared/RAG.Core/Search/
git commit -m "feat(rag): implement hybrid search with vector + BM25 + semantic reranking"
```

---

### Task 5: Implement DocumentIndexer

**Files:**
- Create: `shared/RAG.Core/Indexing/IDocumentIndexer.cs`
- Create: `shared/RAG.Core/Indexing/DocumentIndexer.cs`

**Step 1: Create interface**

```csharp
namespace RAG.Core.Indexing;

public interface IDocumentIndexer
{
    /// <summary>
    /// Index markdown files from a local directory.
    /// </summary>
    Task IndexDirectoryAsync(string directoryPath, CancellationToken ct = default);

    /// <summary>
    /// Index a single markdown string.
    /// </summary>
    Task IndexMarkdownAsync(string markdown, string source, string? category = null,
        CancellationToken ct = default);

    /// <summary>
    /// Ensure the search index exists with the correct schema.
    /// </summary>
    Task EnsureIndexAsync(CancellationToken ct = default);

    /// <summary>
    /// Delete all documents from a specific source, then re-index.
    /// </summary>
    Task ReindexDirectoryAsync(string directoryPath, CancellationToken ct = default);
}
```

**Step 2: Implement DocumentIndexer**

```csharp
using Azure;
using Azure.Search.Documents;
using Azure.Search.Documents.Indexes;
using Azure.Search.Documents.Indexes.Models;
using Azure.Search.Documents.Models;
using RAG.Core.Chunking;
using RAG.Core.Configuration;
using RAG.Core.Embedding;

namespace RAG.Core.Indexing;

public sealed class DocumentIndexer : IDocumentIndexer
{
    private readonly SearchIndexClient _indexClient;
    private readonly SearchClient _searchClient;
    private readonly IEmbedder _embedder;
    private readonly RagOptions _options;

    public DocumentIndexer(
        SearchIndexClient indexClient,
        SearchClient searchClient,
        IEmbedder embedder,
        RagOptions options)
    {
        _indexClient = indexClient;
        _searchClient = searchClient;
        _embedder = embedder;
        _options = options;
    }

    public async Task EnsureIndexAsync(CancellationToken ct = default)
    {
        var index = new SearchIndex(_options.IndexName)
        {
            Fields = new FieldBuilder().Build(typeof(Models.SearchDocument)),
            VectorSearch = new VectorSearch
            {
                Profiles = { new VectorSearchProfile("default-vector", "default-hnsw") },
                Algorithms = { new HnswAlgorithmConfiguration("default-hnsw")
                {
                    Parameters = new HnswParameters
                    {
                        EfConstruction = 400,
                        M = 4,
                        Metric = VectorSearchAlgorithmMetric.Cosine
                    }
                }}
            },
            SemanticSearch = new SemanticSearch
            {
                Configurations =
                {
                    new SemanticConfiguration("default-semantic", new SemanticPrioritizedFields
                    {
                        TitleField = new SemanticField("title"),
                        ContentFields = { new SemanticField("content"), new SemanticField("headingChain") }
                    })
                }
            }
        };

        await _indexClient.CreateOrUpdateIndexAsync(index, cancellationToken: ct);
    }

    public async Task IndexDirectoryAsync(string directoryPath, CancellationToken ct = default)
    {
        await EnsureIndexAsync(ct);

        var markdownFiles = Directory.GetFiles(directoryPath, "*.md", SearchOption.AllDirectories);
        foreach (var file in markdownFiles)
        {
            var content = await File.ReadAllTextAsync(file, ct);
            var relativePath = Path.GetRelativePath(directoryPath, file).Replace('\\', '/');
            await IndexMarkdownAsync(content, relativePath, ct: ct);
        }
    }

    public async Task IndexMarkdownAsync(string markdown, string source, string? category = null,
        CancellationToken ct = default)
    {
        var chunks = MarkdownChunker.Chunk(markdown, source, category, _options.MaxChunkSize);
        if (chunks.Count == 0) return;

        // Embed all chunks in batch
        var texts = chunks.Select(c => c.Content).ToList();
        var vectors = await _embedder.EmbedBatchAsync(texts, ct);

        // Build search documents
        var documents = chunks.Select((chunk, i) => new Models.SearchDocument
        {
            Id = MarkdownChunker.GenerateChunkId(source, chunk.Metadata.ChunkIndex),
            Content = chunk.Content,
            Title = chunk.Metadata.Title,
            Source = chunk.Metadata.Source,
            Category = chunk.Metadata.Category,
            HeadingChain = chunk.Metadata.HeadingChain,
            ContentVector = vectors[i]
        }).ToList();

        // Upsert in batches of 100
        foreach (var batch in documents.Chunk(100))
        {
            await _searchClient.MergeOrUploadDocumentsAsync(batch, cancellationToken: ct);
        }
    }

    public async Task ReindexDirectoryAsync(string directoryPath, CancellationToken ct = default)
    {
        // Delete existing documents from this directory source
        var markdownFiles = Directory.GetFiles(directoryPath, "*.md", SearchOption.AllDirectories);
        foreach (var file in markdownFiles)
        {
            var relativePath = Path.GetRelativePath(directoryPath, file).Replace('\\', '/');
            // Search for documents with this source
            var searchOptions = new Azure.Search.Documents.SearchOptions
            {
                Filter = $"source eq '{relativePath}'",
                Select = { "id" }
            };
            var response = await _searchClient.SearchAsync<Models.SearchDocument>("*", searchOptions, ct);
            var idsToDelete = new List<string>();
            await foreach (var result in response.Value.GetResultsAsync())
                idsToDelete.Add(result.Document.Id);

            if (idsToDelete.Count > 0)
            {
                foreach (var batch in idsToDelete.Chunk(100))
                {
                    await _searchClient.DeleteDocumentsAsync(
                        "id", batch, cancellationToken: ct);
                }
            }
        }

        // Re-index
        await IndexDirectoryAsync(directoryPath, ct);
    }
}
```

**Step 3: Verify it builds**

Run: `dotnet build shared/RAG.Core/RAG.Core.csproj`
Expected: Build succeeded

**Step 4: Commit**

```bash
git add shared/RAG.Core/Indexing/
git commit -m "feat(rag): implement document indexer (chunk → embed → upsert pipeline)"
```

---

### Task 6: Implement DocsCrawler and HtmlToMarkdownConverter

**Files:**
- Create: `shared/RAG.Core/Crawling/DocsCrawler.cs`
- Create: `shared/RAG.Core/Crawling/HtmlToMarkdownConverter.cs`

**Step 1: Implement HtmlToMarkdownConverter**

```csharp
using System.Text;
using AngleSharp;
using AngleSharp.Dom;

namespace RAG.Core.Crawling;

public static class HtmlToMarkdownConverter
{
    private static readonly string[] NoiseTags = ["nav", "footer", "header", "aside", "script", "style"];
    private static readonly string[] NoiseClasses = ["sidebar", "toc", "breadcrumb", "menu", "navigation"];

    public static async Task<string> ConvertAsync(string html, CancellationToken ct = default)
    {
        var config = AngleSharp.Configuration.Default;
        var context = BrowsingContext.New(config);
        var document = await context.OpenAsync(req => req.Content(html), ct);

        // Remove noise elements
        foreach (var tag in NoiseTags)
            foreach (var el in document.QuerySelectorAll(tag).ToList())
                el.Remove();

        foreach (var className in NoiseClasses)
            foreach (var el in document.QuerySelectorAll($"[class*='{className}']").ToList())
                el.Remove();

        // Find main content
        var main = document.QuerySelector("main")
                ?? document.QuerySelector("article")
                ?? document.Body;

        if (main is null)
            return string.Empty;

        var sb = new StringBuilder();
        ConvertNode(main, sb, 0);
        return sb.ToString().Trim();
    }

    private static void ConvertNode(INode node, StringBuilder sb, int depth)
    {
        switch (node)
        {
            case IElement element:
                var tagName = element.TagName.ToLowerInvariant();
                switch (tagName)
                {
                    case "h1": sb.Append("# "); AppendTextContent(element, sb); sb.AppendLine(); sb.AppendLine(); break;
                    case "h2": sb.Append("## "); AppendTextContent(element, sb); sb.AppendLine(); sb.AppendLine(); break;
                    case "h3": sb.Append("### "); AppendTextContent(element, sb); sb.AppendLine(); sb.AppendLine(); break;
                    case "h4": sb.Append("#### "); AppendTextContent(element, sb); sb.AppendLine(); sb.AppendLine(); break;
                    case "h5": sb.Append("##### "); AppendTextContent(element, sb); sb.AppendLine(); sb.AppendLine(); break;
                    case "h6": sb.Append("###### "); AppendTextContent(element, sb); sb.AppendLine(); sb.AppendLine(); break;
                    case "p": AppendTextContent(element, sb); sb.AppendLine(); sb.AppendLine(); break;
                    case "pre":
                        var code = element.QuerySelector("code");
                        var lang = code?.GetAttribute("class")?.Replace("language-", "") ?? "";
                        sb.AppendLine($"```{lang}");
                        sb.AppendLine(element.TextContent.Trim());
                        sb.AppendLine("```");
                        sb.AppendLine();
                        break;
                    case "code" when element.ParentElement?.TagName.ToLowerInvariant() != "pre":
                        sb.Append('`'); sb.Append(element.TextContent); sb.Append('`');
                        break;
                    case "li":
                        sb.Append("- ");
                        AppendTextContent(element, sb);
                        sb.AppendLine();
                        break;
                    case "a":
                        var href = element.GetAttribute("href");
                        sb.Append('['); AppendTextContent(element, sb); sb.Append($"]({href})");
                        break;
                    case "strong" or "b":
                        sb.Append("**"); AppendTextContent(element, sb); sb.Append("**");
                        break;
                    case "em" or "i":
                        sb.Append('*'); AppendTextContent(element, sb); sb.Append('*');
                        break;
                    case "br":
                        sb.AppendLine();
                        break;
                    case "img":
                        // Skip images
                        break;
                    default:
                        foreach (var child in element.ChildNodes)
                            ConvertNode(child, sb, depth + 1);
                        break;
                }
                break;

            case { NodeType: NodeType.Text }:
                var text = node.TextContent;
                if (!string.IsNullOrWhiteSpace(text))
                    sb.Append(text.Trim());
                break;
        }
    }

    private static void AppendTextContent(IElement element, StringBuilder sb)
    {
        foreach (var child in element.ChildNodes)
            ConvertNode(child, sb, 0);
    }
}
```

**Step 2: Implement DocsCrawler**

```csharp
namespace RAG.Core.Crawling;

public sealed class DocsCrawler
{
    private readonly HttpClient _httpClient;
    private readonly int _maxPages;

    public DocsCrawler(HttpClient httpClient, int maxPages = 100)
    {
        _httpClient = httpClient;
        _maxPages = maxPages;
    }

    /// <summary>
    /// Crawls a seed URL and all same-prefix linked pages (depth 1).
    /// Returns a list of (url, markdown) pairs.
    /// </summary>
    public async Task<IReadOnlyList<(string Url, string Markdown)>> CrawlAsync(
        string seedUrl, CancellationToken ct = default)
    {
        var seedUri = new Uri(seedUrl);
        var pathPrefix = seedUri.AbsolutePath;

        // 1. Fetch seed page
        var seedHtml = await FetchAsync(seedUrl, ct);
        if (seedHtml is null)
            return [];

        // 2. Extract same-prefix links
        var links = await ExtractLinksAsync(seedHtml, seedUri, pathPrefix, ct);

        // 3. Fetch linked pages in parallel (max 5 concurrent)
        var results = new List<(string Url, string Markdown)>();
        var seedMarkdown = await HtmlToMarkdownConverter.ConvertAsync(seedHtml, ct);
        if (!string.IsNullOrWhiteSpace(seedMarkdown))
            results.Add((seedUrl, seedMarkdown));

        using var semaphore = new SemaphoreSlim(5);
        var tasks = links.Take(_maxPages - 1).Select(async link =>
        {
            await semaphore.WaitAsync(ct);
            try
            {
                var html = await FetchAsync(link, ct);
                if (html is null) return ((string Url, string Markdown)?)null;

                var md = await HtmlToMarkdownConverter.ConvertAsync(html, ct);
                return string.IsNullOrWhiteSpace(md) ? null : (link, md);
            }
            finally
            {
                semaphore.Release();
            }
        }).ToList();

        var fetched = await Task.WhenAll(tasks);
        results.AddRange(fetched.Where(r => r.HasValue).Select(r => r!.Value));

        return results;
    }

    private async Task<string?> FetchAsync(string url, CancellationToken ct)
    {
        try
        {
            var response = await _httpClient.GetAsync(url, ct);
            if (!response.IsSuccessStatusCode) return null;
            return await response.Content.ReadAsStringAsync(ct);
        }
        catch (HttpRequestException)
        {
            return null;
        }
    }

    private static async Task<List<string>> ExtractLinksAsync(
        string html, Uri seedUri, string pathPrefix, CancellationToken ct)
    {
        var config = AngleSharp.Configuration.Default;
        var context = AngleSharp.BrowsingContext.New(config);
        var document = await context.OpenAsync(req => req.Content(html), ct);

        var links = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var anchor in document.QuerySelectorAll("a[href]"))
        {
            var href = anchor.GetAttribute("href");
            if (string.IsNullOrEmpty(href) || href.StartsWith('#')) continue;

            // Resolve relative URLs
            if (Uri.TryCreate(seedUri, href, out var resolved) &&
                resolved.Host == seedUri.Host &&
                resolved.AbsolutePath.StartsWith(pathPrefix, StringComparison.OrdinalIgnoreCase))
            {
                // Normalize: remove fragment, ensure no trailing slash duplication
                var normalized = $"{resolved.Scheme}://{resolved.Host}{resolved.AbsolutePath}";
                links.Add(normalized);
            }
        }

        // Remove the seed URL itself
        links.Remove($"{seedUri.Scheme}://{seedUri.Host}{seedUri.AbsolutePath}");

        return links.ToList();
    }
}
```

**Step 3: Verify it builds**

Run: `dotnet build shared/RAG.Core/RAG.Core.csproj`
Expected: Build succeeded

**Step 4: Commit**

```bash
git add shared/RAG.Core/Crawling/
git commit -m "feat(rag): implement docs crawler with HTML-to-markdown conversion"
```

---

### Task 7: Implement DependencyInjection and wire up RAG.Core

**Files:**
- Create: `shared/RAG.Core/DependencyInjection.cs`

**Step 1: Implement the DI extension**

```csharp
using Azure;
using Azure.Search.Documents;
using Azure.Search.Documents.Indexes;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RAG.Core.Configuration;
using RAG.Core.Embedding;
using RAG.Core.Indexing;
using RAG.Core.Search;

namespace RAG.Core;

public static class DependencyInjection
{
    public static IServiceCollection AddRagCore(
        this IServiceCollection services,
        IConfigurationSection configSection)
    {
        var options = new RagOptions
        {
            SearchEndpoint = configSection["SearchEndpoint"]
                ?? throw new InvalidOperationException("Rag:SearchEndpoint is required"),
            SearchApiKey = configSection["SearchApiKey"],
            IndexName = configSection["IndexName"] ?? "enterprise-kb",
            EmbeddingEndpoint = configSection["EmbeddingEndpoint"]
                ?? "https://models.inference.ai.azure.com",
            EmbeddingApiKey = configSection["EmbeddingApiKey"],
            EmbeddingModel = configSection["EmbeddingModel"] ?? "text-embedding-3-small",
            UseSemanticRanker = bool.TryParse(configSection["UseSemanticRanker"], out var usr) && usr,
            MaxChunkSize = int.TryParse(configSection["MaxChunkSize"], out var mcs) ? mcs : 1500,
            MaxCrawlPages = int.TryParse(configSection["MaxCrawlPages"], out var mcp) ? mcp : 100
        };

        services.AddSingleton(options);

        // Search clients
        var searchCredential = new AzureKeyCredential(
            options.SearchApiKey
            ?? throw new InvalidOperationException("Rag:SearchApiKey is required"));

        services.AddSingleton(_ => new SearchIndexClient(
            new Uri(options.SearchEndpoint), searchCredential));

        services.AddSingleton(sp => sp.GetRequiredService<SearchIndexClient>()
            .GetSearchClient(options.IndexName));

        // Embedder
        services.AddSingleton<IEmbedder>(_ => new AzureOpenAIEmbedder(
            options.EmbeddingEndpoint,
            options.EmbeddingApiKey
                ?? throw new InvalidOperationException("Rag:EmbeddingApiKey is required"),
            options.EmbeddingModel));

        // Services
        services.AddSingleton<IHybridSearchService, HybridSearchService>();
        services.AddSingleton<IDocumentIndexer, DocumentIndexer>();

        return services;
    }
}
```

**Step 2: Verify it builds**

Run: `dotnet build shared/RAG.Core/RAG.Core.csproj`
Expected: Build succeeded

**Step 3: Commit**

```bash
git add shared/RAG.Core/DependencyInjection.cs
git commit -m "feat(rag): add DI registration for RAG.Core services"
```

---

### Task 8: Create RAG.Indexer CLI tool

**Files:**
- Create: `shared/RAG.Indexer/RAG.Indexer.csproj`
- Create: `shared/RAG.Indexer/Program.cs`
- Modify: `track-3-enterprise-agents.slnx` — add RAG.Indexer project

**Step 1: Create the csproj**

```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net10.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
    <RootNamespace>RAG.Indexer</RootNamespace>
  </PropertyGroup>

  <ItemGroup>
    <ProjectReference Include="../RAG.Core/RAG.Core.csproj" />
  </ItemGroup>

  <ItemGroup>
    <PackageReference Include="Microsoft.Extensions.Configuration.Json" Version="10.0.0" />
    <PackageReference Include="Microsoft.Extensions.Configuration.UserSecrets" Version="10.0.0" />
  </ItemGroup>

</Project>
```

Note: Check `dotnet list package` for actual M.E.Configuration versions matching net10.0. Use whatever version is current.

**Step 2: Implement Program.cs**

```csharp
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RAG.Core;
using RAG.Core.Crawling;
using RAG.Core.Indexing;

// Parse arguments
var source = GetArg(args, "--source")
    ?? throw new InvalidOperationException("--source is required (URL or local path)");
var indexName = GetArg(args, "--index") ?? "enterprise-kb";
var reindex = args.Contains("--reindex");

// Build configuration
var config = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json", optional: true)
    .AddUserSecrets<Program>(optional: true)
    .AddEnvironmentVariables()
    .Build();

// Override index name from CLI arg
config["Rag:IndexName"] = indexName;

// Build DI container
var services = new ServiceCollection();
services.AddRagCore(config.GetSection("Rag"));
services.AddHttpClient();
var sp = services.BuildServiceProvider();

var indexer = sp.GetRequiredService<IDocumentIndexer>();

// Determine source type: URL or local directory
if (Uri.TryCreate(source, UriKind.Absolute, out var uri) && uri.Scheme.StartsWith("http"))
{
    // Web crawl mode
    Console.WriteLine($"Crawling {source}...");
    var crawler = new DocsCrawler(sp.GetRequiredService<IHttpClientFactory>().CreateClient(),
        int.TryParse(config["Rag:MaxCrawlPages"], out var max) ? max : 100);
    var pages = await crawler.CrawlAsync(source);
    Console.WriteLine($"Fetched {pages.Count} pages. Indexing...");

    await indexer.EnsureIndexAsync();
    foreach (var (url, markdown) in pages)
    {
        await indexer.IndexMarkdownAsync(markdown, url);
        Console.WriteLine($"  Indexed: {url}");
    }
}
else
{
    // Local directory mode
    var dirPath = Path.GetFullPath(source);
    if (!Directory.Exists(dirPath))
        throw new DirectoryNotFoundException($"Directory not found: {dirPath}");

    Console.WriteLine($"Indexing {dirPath} → index '{indexName}'...");

    if (reindex)
        await indexer.ReindexDirectoryAsync(dirPath);
    else
        await indexer.IndexDirectoryAsync(dirPath);
}

Console.WriteLine("Done.");
return;

static string? GetArg(string[] args, string name)
{
    var index = Array.IndexOf(args, name);
    return index >= 0 && index + 1 < args.Length ? args[index + 1] : null;
}

// Needed for user-secrets
internal partial class Program;
```

**Step 3: Create appsettings.json for the indexer**

Create `shared/RAG.Indexer/appsettings.json`:

```json
{
  "Rag": {
    "SearchEndpoint": "https://YOUR-SEARCH-SERVICE.search.windows.net",
    "IndexName": "enterprise-kb",
    "EmbeddingEndpoint": "https://models.inference.ai.azure.com",
    "EmbeddingModel": "text-embedding-3-small"
  }
}
```

**Step 4: Add to solution**

Update `track-3-enterprise-agents.slnx` to include `RAG.Indexer`:

```xml
<Project Path="shared/RAG.Indexer/RAG.Indexer.csproj"/>
```

**Step 5: Verify it builds**

Run: `dotnet build shared/RAG.Indexer/RAG.Indexer.csproj`
Expected: Build succeeded

**Step 6: Commit**

```bash
git add shared/RAG.Indexer/ track-3-enterprise-agents.slnx
git commit -m "feat(rag): add CLI indexer tool for local and web docs"
```

---

### Task 9: Integrate RAG.Core into KnowledgeAgent

**Files:**
- Modify: `track-3-enterprise-agents/src/EnterpriseSupportAgent/EnterpriseSupportAgent.csproj` — add RAG.Core reference
- Modify: `track-3-enterprise-agents/src/EnterpriseSupportAgent/Program.cs` — add RAG.Core DI
- Modify: `track-3-enterprise-agents/src/EnterpriseSupportAgent/Agents/KnowledgeAgent.cs` — replace keyword matching

**Step 1: Add project reference to EnterpriseSupportAgent.csproj**

Add to the `<ItemGroup>` with PackageReferences:

```xml
<ProjectReference Include="../../../shared/RAG.Core/RAG.Core.csproj" />
```

**Step 2: Add RAG.Core DI to Program.cs**

After the existing `builder.Services.AddSingleton(sp => ChatClientFactory.CreateChatClientService(builder.Configuration));` line, add:

```csharp
// Add RAG.Core hybrid search
builder.Services.AddRagCore(builder.Configuration.GetSection("Rag"));
```

Add the using at the top:
```csharp
using RAG.Core;
```

**Step 3: Rewrite KnowledgeAgent.HandleKnowledgeQueryAsync**

The `KnowledgeAgent` constructor changes to inject `IHybridSearchService`. The observability MCP routing (`HandleObservabilityQueryAsync`) stays unchanged. Only `HandleKnowledgeQueryAsync` and its helper methods are replaced.

New `KnowledgeAgent.cs` (full file — replaces current content):

```csharp
using System.Text;
using System.Text.Json;
using EnterpriseSupportAgent.AdaptiveCards;
using EnterpriseSupportAgent.Models;
using EnterpriseSupportAgent.Services;
using Microsoft.Agents.Builder;
using Microsoft.Agents.Builder.State;
using OpenAI.Chat;
using RAG.Core.Search;

namespace EnterpriseSupportAgent.Agents;

public sealed class KnowledgeAgent : IConnectedAgent
{
    private static readonly string[] ObservabilityTerms =
    [
        "trace", "traces", "span", "spans", "metric", "metrics",
        "observability", "telemetry", "otlp", "otel", "opentelemetry",
        "latency", "throughput", "error rate", "duration",
        "service map", "genai", "gen_ai",
        "log record", "log records",
        "http error", "http errors", "status code",
        "mcp.qyl", "qyl"
    ];

    private readonly IChatClientService _chatClient;
    private readonly McpObservabilityService _mcpService;
    private readonly IHybridSearchService _searchService;

    public KnowledgeAgent(
        IChatClientService chatClient,
        McpObservabilityService mcpService,
        IHybridSearchService searchService)
    {
        _chatClient = chatClient;
        _mcpService = mcpService;
        _searchService = searchService;
    }

    public string Name => "KnowledgeAgent";

    public async Task<AgentResponse> HandleAsync(string input, ITurnContext turnContext, ITurnState turnState,
        CancellationToken ct)
    {
        // Route: observability questions → MCP tools, everything else → hybrid search KB
        if (IsObservabilityQuery(input))
        {
            var mcpResult = await HandleObservabilityQueryAsync(input, ct);
            if (mcpResult is not null)
                return mcpResult;
        }

        return await HandleKnowledgeQueryAsync(input, ct);
    }

    private static bool IsObservabilityQuery(string input)
    {
        var lower = input.ToLowerInvariant();
        return ObservabilityTerms.Any(term => lower.Contains(term, StringComparison.Ordinal));
    }

    private async Task<AgentResponse?> HandleObservabilityQueryAsync(string input, CancellationToken ct)
    {
        var toolDescriptions = await _mcpService.GetToolDescriptionsAsync(ct);
        if (toolDescriptions is null) return null;

        var toolSelectionMessages = new List<ChatMessage>
        {
            new SystemChatMessage($"""
                You are a tool-calling assistant. Given the user's observability question and the available MCP tools,
                select the best tool and provide arguments as JSON.

                Available tools:
                {toolDescriptions}

                Respond with ONLY a JSON object:
                {{"tool": "tool_name", "arguments": {{"param1": "value1"}}}}

                If no tool is appropriate, respond with:
                {{"tool": "none"}}
                """),
            new UserChatMessage(input)
        };

        var toolCallRaw = await CompleteChatAsync(toolSelectionMessages, ct);
        var (toolName, arguments) = ParseToolCall(toolCallRaw);
        if (toolName is null or "none") return null;

        var toolResult = await _mcpService.CallToolAsync(toolName, arguments, ct);
        if (toolResult is null) return null;

        var answerMessages = new List<ChatMessage>
        {
            new SystemChatMessage($"""
                You are an enterprise observability assistant. The user asked about their system's
                telemetry data. You called the "{toolName}" tool and got the following result.

                Provide a clear, helpful answer based on this data. Summarize key findings.
                Highlight any anomalies, errors, or notable patterns.

                Tool result:
                {toolResult}
                """),
            new UserChatMessage(input)
        };

        var answer = await CompleteChatAsync(answerMessages, ct);

        var card = AdaptiveCardHelper.CreateResultCard(
            "Observability Insight",
            input.Length > 60 ? input[..60] + "..." : input,
            "Live Data", "Good", answer,
            [("Source", "mcp.qyl.info"), ("Tool", toolName), ("", "")]);

        return new AgentResponse(answer, card);
    }

    private async Task<AgentResponse> HandleKnowledgeQueryAsync(string input, CancellationToken ct)
    {
        // 1. Hybrid search (vector + BM25 + semantic reranking)
        var results = await _searchService.SearchAsync(input, ct: ct);

        // 2. Build grounded prompt with citations
        var context = results.Count > 0
            ? string.Join("\n\n---\n\n", results.Select(r =>
                $"**Source: {r.Source} > {r.HeadingChain}**\n{r.Content}"))
            : "No relevant documents found in the knowledge base.";

        var messages = new List<ChatMessage>
        {
            new SystemChatMessage($"""
                You are an enterprise IT support knowledge assistant. Answer the user's question using ONLY
                the provided context. If the answer is not in the context, say "I don't have information about that
                in the knowledge base." Always cite sources using [Source Name] format.

                Context:
                {context}
                """),
            new UserChatMessage(input)
        };

        var answer = await CompleteChatAsync(messages, ct);

        // 3. Build facts list for Adaptive Card
        var factsList = results.Take(3)
            .Select(r => (r.Source, r.Content.Length > 80 ? r.Content[..80] + "..." : r.Content))
            .ToList();
        while (factsList.Count < 3)
            factsList.Add(("", ""));

        var card = AdaptiveCardHelper.CreateResultCard(
            "Knowledge Base Result",
            input.Length > 60 ? input[..60] + "..." : input,
            results.Count > 0 ? "Found" : "No Results",
            results.Count > 0 ? "Good" : "Warning",
            answer, factsList);

        return new AgentResponse(answer, card);
    }

    private static (string? tool, Dictionary<string, object?>? args) ParseToolCall(string llmOutput)
    {
        var json = llmOutput.Trim();
        if (json.StartsWith("```"))
        {
            var lines = json.Split('\n');
            json = string.Join('\n', lines.Skip(1).TakeWhile(l => !l.StartsWith("```")));
        }

        var start = json.IndexOf('{');
        var end = json.LastIndexOf('}');
        if (start < 0 || end <= start) return (null, null);
        json = json[start..(end + 1)];

        using var doc = JsonDocument.Parse(json);
        var tool = doc.RootElement.GetProperty("tool").GetString();

        Dictionary<string, object?>? arguments = null;
        if (doc.RootElement.TryGetProperty("arguments", out var argsElement) &&
            argsElement.ValueKind == JsonValueKind.Object)
        {
            arguments = new Dictionary<string, object?>();
            foreach (var prop in argsElement.EnumerateObject())
            {
                arguments[prop.Name] = prop.Value.ValueKind switch
                {
                    JsonValueKind.String => prop.Value.GetString(),
                    JsonValueKind.Number when prop.Value.TryGetInt64(out var l) => l,
                    JsonValueKind.Number => prop.Value.GetDouble(),
                    JsonValueKind.True => true,
                    JsonValueKind.False => false,
                    JsonValueKind.Null => null,
                    _ => prop.Value.GetRawText()
                };
            }
        }

        return (tool, arguments);
    }

    private async Task<string> CompleteChatAsync(List<ChatMessage> messages, CancellationToken ct)
    {
        var response = _chatClient.CompleteChatStreamingAsync(messages, ct);
        var result = new StringBuilder();
        await foreach (var chunk in response)
        foreach (var part in chunk.ContentUpdate)
            if (!string.IsNullOrEmpty(part.Text))
                result.Append(part.Text);
        return result.ToString();
    }
}
```

**Step 4: Update Program.cs KnowledgeAgent registration**

In `Program.cs`, the `KnowledgeAgent` registration already uses constructor injection. Since `IHybridSearchService` is now registered via `AddRagCore()`, the existing `builder.Services.AddSingleton<KnowledgeAgent>()` line will auto-resolve the new dependency. No change needed to the registration line itself.

**Step 5: Verify it builds**

Run: `dotnet build track-3-enterprise-agents/src/EnterpriseSupportAgent/EnterpriseSupportAgent.csproj`
Expected: Build succeeded

**Step 6: Commit**

```bash
git add track-3-enterprise-agents/src/EnterpriseSupportAgent/
git commit -m "feat(track3): integrate RAG.Core hybrid search into KnowledgeAgent

Replaces naive keyword matching with Azure AI Search hybrid retrieval
(vector + BM25 + semantic reranking). MCP observability routing unchanged."
```

---

### Task 10: Add auto-index hosted service

**Files:**
- Create: `shared/RAG.Core/Indexing/AutoIndexHostedService.cs`
- Modify: `shared/RAG.Core/DependencyInjection.cs` — add `AddRagAutoIndex` method

**Step 1: Create AutoIndexOptions**

Add to `shared/RAG.Core/Configuration/RagOptions.cs`:

```csharp
public sealed class AutoIndexOptions
{
    public required string IndexName { get; init; }
    public required string SourceDirectory { get; init; }
    public bool SkipIfPopulated { get; init; } = true;
}
```

**Step 2: Implement AutoIndexHostedService**

```csharp
using System.Reflection;
using Azure.Search.Documents;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RAG.Core.Configuration;

namespace RAG.Core.Indexing;

public sealed class AutoIndexHostedService : IHostedService
{
    private readonly IDocumentIndexer _indexer;
    private readonly SearchClient _searchClient;
    private readonly AutoIndexOptions _options;
    private readonly ILogger<AutoIndexHostedService> _logger;

    public AutoIndexHostedService(
        IDocumentIndexer indexer,
        SearchClient searchClient,
        AutoIndexOptions options,
        ILogger<AutoIndexHostedService> logger)
    {
        _indexer = indexer;
        _searchClient = searchClient;
        _options = options;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken ct)
    {
        if (_options.SkipIfPopulated)
        {
            try
            {
                var response = await _searchClient.SearchAsync<Models.SearchDocument>(
                    "*",
                    new Azure.Search.Documents.SearchOptions { Size = 1, Select = { "id" } },
                    ct);

                var hasDocuments = false;
                await foreach (var _ in response.Value.GetResultsAsync())
                {
                    hasDocuments = true;
                    break;
                }

                if (hasDocuments)
                {
                    _logger.LogInformation("Index '{Index}' already populated, skipping auto-index",
                        _options.IndexName);
                    return;
                }
            }
            catch
            {
                // Index doesn't exist yet — proceed with indexing
            }
        }

        _logger.LogInformation("Auto-indexing embedded resources from '{Source}' into '{Index}'...",
            _options.SourceDirectory, _options.IndexName);

        await _indexer.EnsureIndexAsync(ct);

        // Load markdown from embedded resources
        var assembly = Assembly.GetEntryAssembly();
        if (assembly is null) return;

        var resourceNames = assembly.GetManifestResourceNames()
            .Where(n => n.EndsWith(".md", StringComparison.OrdinalIgnoreCase));

        var count = 0;
        foreach (var resourceName in resourceNames)
        {
            using var stream = assembly.GetManifestResourceStream(resourceName);
            if (stream is null) continue;

            using var reader = new StreamReader(stream);
            var content = await reader.ReadToEndAsync(ct);

            // Derive source name from resource name
            var parts = resourceName.Split('.');
            var source = parts.Length >= 3
                ? string.Join("/", parts.Skip(2).Take(parts.Length - 3))
                : resourceName;

            await _indexer.IndexMarkdownAsync(content, source, ct: ct);
            count++;
        }

        _logger.LogInformation("Auto-indexed {Count} documents into '{Index}'", count, _options.IndexName);
    }

    public Task StopAsync(CancellationToken ct) => Task.CompletedTask;
}
```

**Step 3: Add `AddRagAutoIndex` to DependencyInjection.cs**

Add this method to the existing `DependencyInjection` class:

```csharp
public static IServiceCollection AddRagAutoIndex(
    this IServiceCollection services,
    Action<AutoIndexOptions> configure)
{
    var options = new AutoIndexOptions { IndexName = "enterprise-kb", SourceDirectory = "Knowledge" };
    configure(options);
    services.AddSingleton(options);
    services.AddHostedService<AutoIndexHostedService>();
    return services;
}
```

**Step 4: Verify it builds**

Run: `dotnet build shared/RAG.Core/RAG.Core.csproj`
Expected: Build succeeded

**Step 5: Commit**

```bash
git add shared/RAG.Core/Indexing/AutoIndexHostedService.cs shared/RAG.Core/DependencyInjection.cs shared/RAG.Core/Configuration/
git commit -m "feat(rag): add auto-index hosted service for startup indexing"
```

---

### Task 11: End-to-end build verification and cleanup

**Files:**
- Verify: all projects build
- Verify: solution loads correctly

**Step 1: Build entire solution**

Run: `dotnet build track-3-enterprise-agents.slnx`
Expected: Build succeeded, 0 errors

**Step 2: Fix any build errors**

If there are type conflicts (e.g., `SearchOptions` ambiguity between `RAG.Core.Search.RagSearchOptions` and `Azure.Search.Documents.SearchOptions`), resolve with explicit namespacing. The plan uses `RagSearchOptions` for the RAG type and fully-qualified `Azure.Search.Documents.SearchOptions` in the implementation to avoid this.

**Step 3: Verify appsettings setup instructions**

Create/update `track-3-enterprise-agents/src/EnterpriseSupportAgent/.env.example`:

```
Rag__SearchEndpoint=https://YOUR-SEARCH-SERVICE.search.windows.net
Rag__SearchApiKey=YOUR-SEARCH-API-KEY
Rag__EmbeddingEndpoint=https://models.inference.ai.azure.com
Rag__EmbeddingApiKey=YOUR-GITHUB-TOKEN
Rag__IndexName=enterprise-kb
Rag__UseSemanticRanker=true
```

**Step 4: Final commit**

```bash
git add -A
git commit -m "feat(rag): complete RAG.Core integration — build verified

RAG.Core provides:
- Heading-aware markdown chunker
- Azure OpenAI embeddings (text-embedding-3-small)
- Azure AI Search hybrid search (vector + BM25 + semantic reranker)
- Document indexer (chunk → embed → upsert)
- Docs crawler (HTML → markdown, depth-1)
- CLI indexer tool
- Auto-index hosted service

KnowledgeAgent now uses hybrid search instead of keyword matching."
```

---

## Dependency Graph

```
Task 1: Models + csproj       (no deps)
Task 2: MarkdownChunker       (depends on Task 1 models)
Task 3: Embedder              (depends on Task 1 csproj)
Task 4: HybridSearchService   (depends on Task 1, 3)
Task 5: DocumentIndexer       (depends on Task 1, 2, 3)
Task 6: DocsCrawler           (depends on Task 1 csproj)
Task 7: DependencyInjection   (depends on Task 3, 4, 5)
Task 8: CLI Indexer            (depends on Task 5, 6, 7)
Task 9: KnowledgeAgent rewrite (depends on Task 4, 7)
Task 10: AutoIndex service     (depends on Task 5, 7)
Task 11: Build verification    (depends on all)
```

Tasks 2, 3, 6 can run in parallel after Task 1.
Tasks 4, 5 can run in parallel after Task 3.
Tasks 8, 9, 10 can run in parallel after Task 7.
