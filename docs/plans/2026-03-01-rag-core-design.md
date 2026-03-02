# RAG.Core Design — Hybrid Search Pipeline for Agents League

**Date:** 2026-03-01
**Status:** Approved
**Tracks:** Track 3 (primary), Track 2 (future consumer)

## Problem

Track 3's `KnowledgeAgent` uses naive keyword matching over 25 embedded markdown files.
It misses semantic matches, returns low-quality excerpts, and cannot scale to external docs.
We need a proper RAG pipeline: chunk → embed → index → hybrid search → rerank → generate.

We also want a reusable "point at any docs URL" system — index any documentation site
and query it with the same pipeline.

## Architecture

```
agentsleague-starter-kits/
  shared/
    RAG.Core/                        # Class library (net10.0)
      Models/
        SearchDocument.cs            # Index schema
        SearchResult.cs              # Query result
        ChunkMetadata.cs             # Heading chain, source, category
      Chunking/
        MarkdownChunker.cs           # Heading-aware splitting
      Embedding/
        AzureOpenAIEmbedder.cs       # text-embedding-3-small
      Search/
        HybridSearchService.cs       # Vector + BM25 + semantic ranker
      Indexing/
        DocumentIndexer.cs           # Chunk → embed → upsert pipeline
      Crawling/
        DocsCrawler.cs               # Fetch URL + same-prefix links (depth 1)
        HtmlToMarkdownConverter.cs   # Strip nav, extract main content
      Configuration/
        RagOptions.cs                # Endpoint, index name, model config
      DependencyInjection.cs         # builder.Services.AddRagCore(config)

    RAG.Indexer/                     # Console app (CLI tool)
      Program.cs                     # --source URL/path --index name

  track-3-enterprise-agents/
    src/EnterpriseSupportAgent/
      Agents/KnowledgeAgent.cs       # MODIFIED: calls HybridSearchService
      Program.cs                     # MODIFIED: adds RAG.Core DI
```

### Dependencies

| Package | Purpose |
|---------|---------|
| `Azure.Search.Documents` | AI Search client (hybrid search, semantic ranker) |
| `Azure.AI.OpenAI` | Embeddings (text-embedding-3-small) |
| `AngleSharp` | HTML parsing for web crawler |

## Search Index Schema

```
Index: "enterprise-kb" (configurable)

Fields:
  id              string    [key, filterable]         # SHA256(source + chunk position)
  content         string    [searchable]              # Chunk text (BM25 target)
  title           string    [searchable, filterable]  # Section heading or page title
  source          string    [filterable, facetable]   # URL or file path
  category        string    [filterable, facetable]   # e.g. "it-solution", "hr"
  headingChain    string    [searchable]              # "# Page > ## Section > ### Sub"
  contentVector   Collection(Single) [1536 dims]      # text-embedding-3-small output

Vector: HNSW (efConstruction: 400, m: 4, metric: Cosine)
Semantic: title + content + headingChain
```

## Chunking Strategy

1. Parse YAML frontmatter → extract metadata (name, persona, tools, category)
2. Split on heading boundaries (`#`, `##`, `###`)
3. Keep code blocks intact (never split mid-fence)
4. If chunk exceeds 1500 chars, split on paragraph boundaries (`\n\n`)
5. Each chunk carries its heading chain as context
6. 1-sentence overlap between adjacent chunks

## Query Pipeline

```
User question
    │
    ├── Embed (text-embedding-3-small) ──→ Vector search (top 50)
    │                                          │
    └── BM25 keyword search (top 50) ──────────┘
                                               │
                                    RRF (Reciprocal Rank Fusion)
                                               │
                                    Semantic Reranker (top 5)
                                               │
                                    Grounded LLM prompt + citations
```

### HybridSearchService API

```csharp
public interface IHybridSearchService
{
    Task<IReadOnlyList<SearchResult>> SearchAsync(
        string query,
        SearchOptions? options = null,
        CancellationToken ct = default);
}

public record SearchResult(
    string Content, string Title, string Source,
    string HeadingChain, double Score);

public record SearchOptions(
    string? CategoryFilter = null,
    int TopK = 5,
    int CandidateCount = 50,
    bool UseSemanticRanker = true);
```

## Crawling (docs URL ingestion)

1. Fetch seed URL
2. Parse HTML with AngleSharp, extract all `<a href>` links
3. Filter: same path prefix only, deduplicate, cap at maxPages (default 100)
4. Fetch linked pages in parallel (semaphore, max 5 concurrent)
5. HtmlToMarkdownConverter: remove nav/footer/sidebar, extract main/article, convert to markdown
6. Local directory path → skip crawler, read files directly

## Integration with KnowledgeAgent

KnowledgeAgent drops from 192 lines to ~60:

- **Deleted:** LoadDocuments, FindRelevantDocuments, FindBestExcerpt, Tokenize, static cache
- **Kept:** HandleAsync shape, chat completion, citation building, Adaptive Card rendering
- **New:** Constructor takes `IHybridSearchService`, calls `SearchAsync()` instead of keyword matching

Program.cs adds:
```csharp
builder.Services.AddRagCore(builder.Configuration.GetSection("Rag"));
```

## CLI Indexer

```bash
# Index from URL
dotnet run --project shared/RAG.Indexer -- \
  --source https://opentelemetry.io/docs/specs/semconv/gen-ai/ \
  --index otel-semconv

# Index local markdown
dotnet run --project shared/RAG.Indexer -- \
  --source ./Knowledge --index enterprise-kb

# Re-index
dotnet run --project shared/RAG.Indexer -- \
  --source ./Knowledge --index enterprise-kb --reindex
```

## Auto-Index at Startup

Optional hosted service for Track 3 demo:
- Checks if index exists and is populated
- If empty, indexes embedded Knowledge/*.md files
- Skips on subsequent restarts (`SkipIfPopulated = true`)

## Configuration

```json
{
  "Rag": {
    "SearchEndpoint": "https://{name}.search.windows.net",
    "IndexName": "enterprise-kb",
    "EmbeddingModel": "text-embedding-3-small",
    "UseSemanticRanker": true
  }
}
```

Search API key and Azure OpenAI key via user-secrets.
