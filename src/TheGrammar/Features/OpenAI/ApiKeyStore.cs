using Meziantou.Framework.Win32;

namespace TheGrammar.Features.OpenAI;

public static class ApiKeyStore
{
  private const string TargetName = "TheGrammar/OpenAI";
  private const string UserName = "ApiKey";

  public static string? Load()
  {
    var credential = CredentialManager.ReadCredential(TargetName);
    return credential?.Password;
  }

  public static void Save(string apiKey)
  {
    CredentialManager.WriteCredential(
      applicationName: TargetName,
      userName: UserName,
      secret: apiKey,
      comment: "TheGrammar OpenAI API Key",
      persistence: CredentialPersistence.LocalMachine);
  }

  public static void Delete()
  {
    CredentialManager.DeleteCredential(TargetName);
  }
}