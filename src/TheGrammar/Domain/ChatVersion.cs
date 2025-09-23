using System.ComponentModel;

namespace TheGrammar.Domain;

public enum ChatVersion
{
  [Description("GPT-5")] Gpt5,
  [Description("GPT-5 Nano")] Gpt5Nano,
  [Description("GPT-4 Turbo")] Turbo4,
  [Description("GPT-4o")] Gpt4o,
}