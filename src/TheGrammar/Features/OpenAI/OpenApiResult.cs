using TheGrammar.Domain;

namespace TheGrammar.Features.OpenAI;

public record OpenApiResult(
  string OriginalText,
  string ModifiedText,
  ChatVersion ChatVersion
);