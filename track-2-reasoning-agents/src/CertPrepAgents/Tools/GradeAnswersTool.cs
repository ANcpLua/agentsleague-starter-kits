using System.ComponentModel;
using System.Text.Json;
using CertPrepAgents.Contracts;
using Microsoft.Extensions.AI;

namespace CertPrepAgents.Tools;

internal static class GradeAnswersTool
{
    [Description("Grades a student's answers to certification exam questions. Returns detailed results with reasoning for each answer.")]
    public static async Task<string> GradeAnswers(
        [Description("JSON array of objects with 'question', 'domain', 'correctAnswer', and 'userAnswer' fields")] string questionsJson,
        IChatClient chatClient)
    {
        var prompt = $"""
            You are an exam grader for Microsoft certifications.
            Grade each question below. For each one, determine if the user's answer matches
            the correct answer (exact letter match). Provide educational reasoning explaining
            why the correct answer is right and why common wrong answers are incorrect.

            Questions to grade:
            {questionsJson}

            Respond with a JSON array of objects, each with:
            - "domain": string
            - "question": string (the question text)
            - "userAnswer": string
            - "correctAnswer": string
            - "isCorrect": boolean
            - "reasoning": string (educational explanation)

            Output ONLY valid JSON array. No markdown fences.
            """;

        var response = await chatClient.GetResponseAsync(prompt);
        return response.Text;
    }
}
