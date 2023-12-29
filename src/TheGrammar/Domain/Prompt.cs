namespace TheGrammar.Domain;

public class Prompt : AuditableEntity
{
    public int Id { get; set; }
    public Keys LeftKey { get; set; } = Keys.Control | Keys.Shift;
    public Keys RightKey { get; set; }
    public required string Promt { get; set; }

    public override string ToString()
    {
        return $"{LeftKey} + {RightKey}";
    }
}