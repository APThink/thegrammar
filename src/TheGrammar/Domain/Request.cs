using TheGrammar.Features.OpenAI;

namespace TheGrammar.Domain;

public class Request : AuditableEntity
{
    public int Id { get; set; }
    public required string RequestText { get; set; }
    public required string ResponseText { get; set; }
    public required ChatVersion? ChatVersion { get; set; }
}
