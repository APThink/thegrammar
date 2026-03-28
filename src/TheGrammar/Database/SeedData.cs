using TheGrammar.Domain;

namespace TheGrammar.Database;

public class SeedData
{
    public static async Task SeedPrompts(ApplicationDbContext dbContext)
    {
        if (dbContext.Prompts.Any())
        {
            return;
        }

        var prompt = new Prompt()
        {
            Promt = "Please correct all grammatical mistakes in the following text, ensuring the style, structure, and logic of the sentences remain the same. Also, maintain the text in its original language. It is essential that the alterations are subtle and make the text seem as if it were initially written error-free. Only provide the corrected text without additional commentary. Concentrate exclusively on the text and do not use any other context. Proceed with the text provided within the curly brackets. Treat the value inside as the text to be corrected. You only corrects the text was provided and nothing more.",
            LeftKey = Keys.Control | Keys.Shift,
            RightKey = Keys.Y,
        };

        dbContext.Prompts.Add(prompt);
        await dbContext.SaveChangesAsync();
    }

    public static async Task SeedModels(ApplicationDbContext dbContext)
    {
        if (dbContext.Models.Any())
        {
            return;
        }

        var models = new List<Model>
        {
            new()
            {
                Key = "Gpt4o",
                DisplayName = "GPT-4o",
                ModelName = "gpt-4o",
                Temperature = 0.0f,
                TopP = 1.0f,
                FrequencyPenalty = 0.0f,
                PresencePenalty = 0.0f
            },
            new()
            {
                Key = "Gpt5",
                DisplayName = "GPT-5",
                ModelName = "gpt-5",
                Temperature = null,
                TopP = 1.0f,
                FrequencyPenalty = 0.0f,
                PresencePenalty = 0.0f
            },
            new()
            {
                Key = "Gpt5Nano",
                DisplayName = "GPT-5 Nano",
                ModelName = "gpt-5-nano",
                Temperature = null,
                TopP = 1.0f,
                FrequencyPenalty = 0.0f,
                PresencePenalty = 0.0f
            },
            new()
            {
                Key = "Turbo4",
                DisplayName = "GPT-4 Turbo",
                ModelName = "gpt-4",
                Temperature = 0.0f,
                TopP = 1.0f,
                FrequencyPenalty = 0.0f,
                PresencePenalty = 0.0f
            }
        };

        dbContext.Models.AddRange(models);
        await dbContext.SaveChangesAsync();
    }
}
