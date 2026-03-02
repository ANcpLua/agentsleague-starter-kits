using System.ComponentModel;
using Microsoft.Extensions.AI;
using ModelContextProtocol.Server;

internal sealed class CodeIntelligenceTools
{
    [McpServerTool]
    [Description("Analyzes source code to explain its purpose, identify the algorithm used, assess time/space complexity, find potential bugs, and suggest improvements. Returns a structured markdown analysis. Use when a user asks 'what does this code do', 'explain this function', or 'review this code'.")]
    public static async Task<string> ExplainCode(
        [Description("The source code to analyze. Can be any programming language.")] string code,
        [Description("Analysis focus: 'algorithm' for algorithm identification, 'complexity' for Big-O analysis, 'bugs' for bug detection, 'improvements' for refactoring suggestions, or 'all' for comprehensive analysis.")] string focus,
        IChatClient chatClient)
    {
        using var activity = McpActivitySource.StartToolActivity("explain_code", "gpt-4o-mini",
            new() { ["code_intel.focus"] = focus, ["code_intel.code_length"] = code.Length.ToString() });

        var prompt = $"""
            You are a senior software engineer performing code analysis.

            Analyze the following code with focus on: {focus}

            ```
            {code}
            ```

            Provide your analysis in this markdown format:
            ## Language & Purpose
            [Identify language, describe what the code does]

            ## Algorithm
            [Name the algorithm/pattern used, explain the approach]

            ## Complexity
            - **Time:** O(?)
            - **Space:** O(?)

            ## Issues Found
            [List any bugs, edge cases, or problems]

            ## Suggested Improvements
            [Concrete refactoring suggestions with code examples]
            """;

        var response = await chatClient.GetResponseAsync(prompt);
        activity?.SetTag("gen_ai.usage.output_tokens", response.Usage?.OutputTokenCount);
        return response.Text;
    }

    [McpServerTool]
    [Description("Converts source code from one programming language to another while preserving semantics, using idiomatic patterns in the target language. Returns the converted code with explanatory comments. Use when a user says 'convert this to C#', 'translate this Python to Go', etc.")]
    public static async Task<string> ConvertCode(
        [Description("The source code to convert.")] string code,
        [Description("Source programming language (e.g., 'python', 'go', 'javascript', 'java').")] string sourceLanguage,
        [Description("Target programming language (e.g., 'csharp', 'python', 'go', 'rust').")] string targetLanguage,
        IChatClient chatClient)
    {
        using var activity = McpActivitySource.StartToolActivity("convert_code", "gpt-4o-mini",
            new() { ["code_intel.source_lang"] = sourceLanguage, ["code_intel.target_lang"] = targetLanguage });

        var prompt = $"""
            You are an expert polyglot programmer.

            Convert the following {sourceLanguage} code to idiomatic {targetLanguage}:

            ```{sourceLanguage}
            {code}
            ```

            Requirements:
            - Use idiomatic {targetLanguage} patterns and conventions
            - Preserve all functionality and edge case handling
            - Add brief comments explaining non-obvious translations
            - Use the standard library where possible
            - Follow {targetLanguage} naming conventions

            Return ONLY the converted code in a fenced code block.
            """;

        var response = await chatClient.GetResponseAsync(prompt);
        activity?.SetTag("gen_ai.usage.output_tokens", response.Usage?.OutputTokenCount);
        return response.Text;
    }

    [McpServerTool]
    [Description("Generates executable, parameterized code that solves a described problem. The code includes input validation, error handling, and is ready to run. Use when a user describes a problem in natural language and wants working code.")]
    public static async Task<string> SolveWithCode(
        [Description("Natural language description of the problem to solve.")] string problem,
        [Description("Target programming language for the solution (e.g., 'csharp', 'python').")] string language,
        IChatClient chatClient)
    {
        using var activity = McpActivitySource.StartToolActivity("solve_with_code", "gpt-4o-mini",
            new() { ["code_intel.language"] = language });

        var prompt = $"""
            You are a senior developer who writes clean, production-ready code.

            Problem: {problem}

            Write a complete, runnable {language} solution that:
            1. Accepts input parameters (not hardcoded values)
            2. Includes input validation
            3. Handles edge cases
            4. Has clear variable names and minimal comments
            5. Includes a usage example

            Return the solution as:
            ## Solution
            ```{language}
            [your code here]
            ```

            ## Usage Example
            ```{language}
            [example invocation]
            ```

            ## Approach
            [1-2 sentence explanation of the algorithm chosen and why]
            """;

        var response = await chatClient.GetResponseAsync(prompt);
        activity?.SetTag("gen_ai.usage.output_tokens", response.Usage?.OutputTokenCount);
        return response.Text;
    }

    [McpServerTool]
    [Description("Analyzes a visual description or UI specification to provide feedback on layout, accessibility, design patterns, and suggests code implementation. Use when reviewing UI mockups, screenshots described in text, or design specifications.")]
    public static async Task<string> AnalyzeVisual(
        [Description("Text description of the visual/UI to analyze. Can be an ASCII mockup, design spec, or description of a screenshot.")] string visualDescription,
        [Description("Analysis focus: 'accessibility' for a11y review, 'layout' for structure analysis, 'implementation' for code generation, or 'all'.")] string focus,
        IChatClient chatClient)
    {
        using var activity = McpActivitySource.StartToolActivity("analyze_visual", "gpt-4o-mini",
            new() { ["code_intel.focus"] = focus });

        var prompt = $"""
            You are a UI/UX expert and frontend developer.

            Analyze this visual/UI specification with focus on: {focus}

            Description:
            {visualDescription}

            Provide analysis in this format:

            ## Layout Analysis
            [Structure, hierarchy, spacing, alignment observations]

            ## Accessibility Review
            [WCAG compliance, contrast, keyboard navigation, screen reader considerations]

            ## Design Patterns
            [Which UI patterns are used, alternatives to consider]

            ## Implementation Suggestion
            [Brief code snippet showing how to implement the key components]
            """;

        var response = await chatClient.GetResponseAsync(prompt);
        activity?.SetTag("gen_ai.usage.output_tokens", response.Usage?.OutputTokenCount);
        return response.Text;
    }
}
