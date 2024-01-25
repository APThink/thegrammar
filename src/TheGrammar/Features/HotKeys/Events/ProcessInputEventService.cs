using System.Reactive.Linq;
using System.Reactive.Subjects;
using TheGrammar.Domain;

namespace TheGrammar.Features.HotKeys.Events;

public record ProcessStartDto(string Input, string Prompt);
public record ProcessFinishDto(string OriginalText, string ModifiedText, ChatVersion? ChatVersion);


public interface IProcessInputEventService
{
    IObservable<ProcessStartDto> ProcessStartEvents { get; }
    void TrigerProcessStart(ProcessStartDto processStart);

    IObservable<ProcessFinishDto> ProcessFinishEvents { get; }
    void TrigerProcessFinish(ProcessFinishDto processFinish);
}

public class ProcessInputEventService : IProcessInputEventService
{
    private readonly ISubject<ProcessStartDto> _processStartEventStream = new ReplaySubject<ProcessStartDto>(1);
    public IObservable<ProcessStartDto> ProcessStartEvents => _processStartEventStream.AsObservable();

    private readonly ISubject<ProcessFinishDto> _processFinisEventStream = new ReplaySubject<ProcessFinishDto>(1);
    public IObservable<ProcessFinishDto> ProcessFinishEvents => _processFinisEventStream.AsObservable();

    public void TrigerProcessStart(ProcessStartDto processStart)
    {
        _processStartEventStream.OnNext(processStart);
    }

    public void TrigerProcessFinish(ProcessFinishDto processFinish)
    {
        _processFinisEventStream.OnNext(processFinish);
    }
}
