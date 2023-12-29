namespace TheGrammar.Features.Logging;

public partial class Logging
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
            var fileName = $"TheGrammar{DateTime.Now.Year}{DateTime.Now.Month}.txt";
            var path = Path.Combine(Environment.CurrentDirectory, $"Logs/{fileName}");
            logTextFile = File.ReadAllText(path);
        }
        catch (Exception)
        {
            logTextFile = "File not found";
        }

    }
}
