namespace TheGrammar.Features.OpenAI;

public record OpenApiResult(
  string OriginalText,
  string ModifiedText,
  string ModelKey
);