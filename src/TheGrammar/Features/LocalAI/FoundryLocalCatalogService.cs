using Microsoft.AI.Foundry.Local;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using TheGrammar.Features.Settings;

namespace TheGrammar.Features.LocalAI;

public record LocalModelInfo(string Alias, string DisplayName, int? FileSizeMb, bool IsCached, bool IsLoaded);

public record LocalChatSession(string Alias, OpenAIChatClient ChatClient);

public class FoundryLocalCatalogService(IOptionsMonitor<SettingsOption> settingsMonitor) : IDisposable
{
  private readonly SemaphoreSlim _initLock = new(1, 1);
  private FoundryLocalManager? _manager;

  private async Task<FoundryLocalManager> GetManagerAsync(CancellationToken ct)
  {
    if (_manager is not null)
    {
      return _manager;
    }

    await _initLock.WaitAsync(ct);
    try
    {
      if (_manager is not null)
      {
        return _manager;
      }

      await FoundryLocalManager.CreateAsync(new Configuration { AppName = "TheGrammar" }, NullLogger.Instance, ct);
      _manager = FoundryLocalManager.Instance;
      return _manager;
    }
    finally
    {
      _initLock.Release();
    }
  }

  public async Task<IReadOnlyList<LocalModelInfo>> GetCatalogModelsAsync(CancellationToken ct = default)
  {
    var manager = await GetManagerAsync(ct);
    var catalog = await manager.GetCatalogAsync(ct);
    var models = await catalog.ListModelsAsync(ct);

    var result = new List<LocalModelInfo>(models.Count);
    foreach (var model in models)
    {
      result.Add(new LocalModelInfo(
        model.Alias,
        model.Info.DisplayName,
        model.Info.FileSizeMb,
        await model.IsCachedAsync(ct),
        await model.IsLoadedAsync(ct)));
    }

    return result;
  }

  public async Task DownloadAndLoadAsync(string alias, IProgress<float> progress, CancellationToken ct = default)
  {
    var manager = await GetManagerAsync(ct);
    var catalog = await manager.GetCatalogAsync(ct);
    var model = await catalog.GetModelAsync(alias, ct)
      ?? throw new InvalidOperationException($"Model '{alias}' was not found in the Foundry Local catalog.");

    foreach (var loadedModel in await catalog.GetLoadedModelsAsync(ct))
    {
      await loadedModel.UnloadAsync(ct);
    }

    if (!await model.IsCachedAsync(ct))
    {
      await model.DownloadAsync(p => progress.Report(p), ct);
    }

    await model.LoadAsync(ct);
  }

  public async Task<LocalChatSession> GetChatClientForCurrentModelAsync(CancellationToken ct = default)
  {
    var alias = settingsMonitor.CurrentValue.LocalModelAlias;
    if (string.IsNullOrWhiteSpace(alias))
    {
      throw new InvalidOperationException("No local model selected. Choose one in Settings.");
    }

    var manager = await GetManagerAsync(ct);
    var catalog = await manager.GetCatalogAsync(ct);
    var model = await catalog.GetModelAsync(alias, ct)
      ?? throw new InvalidOperationException($"Selected local model '{alias}' was not found in the Foundry Local catalog.");

    if (!await model.IsLoadedAsync(ct))
    {
      await model.LoadAsync(ct);
    }

    var chatClient = await model.GetChatClientAsync(ct);
    return new LocalChatSession(alias, chatClient);
  }

  public void Dispose()
  {
    _manager?.Dispose();
    _initLock.Dispose();
  }
}
