using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace TheGrammar.Features.HotKeys.Events;

public record UserInput(string Input, string Prompt);

public interface IProcessInputEventService
{
    IObservable<UserInput> Events { get; }

    void TriggerEvent(UserInput userInput);
}

public class ProcessInputEventService : IProcessInputEventService
{
    private readonly ISubject<UserInput> _eventStream = new Subject<UserInput>();

    public IObservable<UserInput> Events => _eventStream.AsObservable();

    public void TriggerEvent(UserInput userInput)
    {
        _eventStream.OnNext(userInput);
    }
}
