namespace TheGrammar.Domain;

public class Model : AuditableEntity
{
    public int Id { get; set; }
    public required string Key { get; set; }
    public required string DisplayName { get; set; }
    public required string ModelName { get; set; }
    public float? Temperature { get; set; }
    public float TopP { get; set; } = 1.0f;
    public float FrequencyPenalty { get; set; } = 0.0f;
    public float PresencePenalty { get; set; } = 0.0f;
}