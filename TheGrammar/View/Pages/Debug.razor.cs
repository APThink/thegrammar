namespace TheGrammar.View.Pages;

public partial class Debug
{
    private string logTextFile = string.Empty;

    protected override void OnInitialized()
    {
        ReadFile();
    }

    private void ReadFile()
    {
        try
        {
            var fileName = $"thegrammar{DateTime.Now.Year}{DateTime.Now.Month}.txt";
            var path = Path.Combine(Environment.CurrentDirectory, $"logs/{fileName}");
            logTextFile = File.ReadAllText(path);
        }
        catch (Exception)
        {
            logTextFile = "File not found";
        }

    }
    
}
