namespace TheGrammar.Data;

public class SeedData
{
    public static async Task SeedPrompts(AppDbContext dbContext)
    {
        if(dbContext.Prompts.Any())
        {
            return;
        }

        var prompt = new Prompt()
        {
            Promt = "Bitte korrigiere alle grammatikalischen Fehler im folgenden Text, wobei der Stil, die Struktur und die Logik der Sätze unverändert bleiben sollen. Behalte auch die Originalsprache des Textes bei. Es ist wichtig, dass die Änderungen subtil sind und der Text natürlich erscheint, als wäre er anfänglich ohne Fehler geschrieben worden. Schreibe nur die korrigierte Antwort ohne zusätzliche Informationen. Konzentriere dich nur auf den Text und benutze keinen anderen Kontext. Text ist: ",
            LeftKey = Keys.Control | Keys.Shift,
            RightKey = Keys.Y,
        };

        dbContext.Prompts.Add(prompt);
        await dbContext.SaveChangesAsync();
    }
}
