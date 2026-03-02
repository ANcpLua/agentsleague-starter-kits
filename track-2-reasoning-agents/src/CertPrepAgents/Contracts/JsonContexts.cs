using System.Text.Json.Serialization;

namespace CertPrepAgents.Contracts;

[JsonSerializable(typeof(CuratorOutput))]
[JsonSerializable(typeof(StudyPlan))]
[JsonSerializable(typeof(AssessmentResult))]
[JsonSerializable(typeof(RemediationPlan))]
[JsonSerializable(typeof(PlannerDecision))]
[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
internal sealed partial class CertPrepJsonContext : JsonSerializerContext;
