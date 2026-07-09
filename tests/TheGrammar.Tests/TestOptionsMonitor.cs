using Microsoft.Extensions.Options;

namespace TheGrammar.Tests;

internal sealed class TestOptionsMonitor<T>(T value) : IOptionsMonitor<T> where T : class
{
  public T CurrentValue { get; } = value;

  public T Get(string? name) => CurrentValue;

  public IDisposable OnChange(Action<T, string?> listener) => null!;
}
