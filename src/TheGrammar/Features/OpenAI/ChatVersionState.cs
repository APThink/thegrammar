using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Serilog;
using TheGrammar.Database;
using TheGrammar.Domain;

namespace TheGrammar.Features.OpenAI;

public class ChatVersionState
{
  private readonly IDbContextFactory<ApplicationDbContext> _dbContextFactory;
  private readonly string _defaultModelKey;
  private string? _currentModelKey;

  public ChatVersionState(IDbContextFactory<ApplicationDbContext> dbContextFactory, IOptions<OpenAiOptions> options)
  {
    _dbContextFactory = dbContextFactory;
    _defaultModelKey = options.Value.DefaultModel;
  }

  public Model CurrentModel
  {
    get
    {
      using var dbContext = _dbContextFactory.CreateDbContext();
      var key = _currentModelKey ?? _defaultModelKey;
      return dbContext.Models.FirstOrDefault(m => m.Key == key)
             ?? dbContext.Models.First();
    }
  }

  public ChatModelSettings Current => new(
    CurrentModel.ModelName,
    CurrentModel.Temperature,
    CurrentModel.TopP,
    CurrentModel.FrequencyPenalty,
    CurrentModel.PresencePenalty
  );

  public List<Model> GetAllModels()
  {
    using var dbContext = _dbContextFactory.CreateDbContext();
    return dbContext.Models.ToList();
  }

  public void SetCurrentModel(string modelKey)
  {
    using var dbContext = _dbContextFactory.CreateDbContext();
    var model = dbContext.Models.FirstOrDefault(m => m.Key == modelKey);
    if (model != null)
    {
      _currentModelKey = modelKey;
    }
    else
    {
      Log.Error("Failed to set current model for key {ModelKey}", modelKey);
    }
  }

  public string GetCurrentModelKey() => _currentModelKey ?? _defaultModelKey;
}
