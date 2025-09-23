namespace TheGrammar.Features.OpenAI;

public record ChatModelSettings(
  string ModelName,
  float? Temperature,
  float? TopP,
  float? FrequencyPenalty,
  float? PresencePenalty
);