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
}
