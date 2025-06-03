using System.Reactive.Linq;
using System.Reactive.Subjects;
using TheGrammar.Domain;

namespace TheGrammar.Features.HotKeys.Events
{
  public record ProcessStartDto(string Input, string Prompt);
  public record ProcessFinishDto(string OriginalText, string ModifiedText, ChatVersion? ChatVersion);
  public record ProcessCancellationDto();
  

  public interface IProcessInputEventService
  {
    IObservable<ProcessStartDto> ProcessStartEvents { get; }
    void TriggerProcessStart(ProcessStartDto processStart);

    IObservable<ProcessFinishDto> ProcessFinishEvents { get; }
    void TriggerProcessFinish(ProcessFinishDto processFinish);

    IObservable<ProcessCancellationDto> ProcessCancellationEvents { get; }
    void TriggerProcessCancellation();
  }

  public class ProcessInputEventService : IProcessInputEventService, IObservable<ProcessCancellationDto>
  {
    private readonly ISubject<ProcessStartDto> _processStartEventStream = new ReplaySubject<ProcessStartDto>(1);
    public IObservable<ProcessStartDto> ProcessStartEvents => _processStartEventStream.AsObservable();

    private readonly ISubject<ProcessFinishDto> _processFinishEventStream = new ReplaySubject<ProcessFinishDto>(1);
    public IObservable<ProcessFinishDto> ProcessFinishEvents => _processFinishEventStream.AsObservable();

    private readonly ISubject<ProcessCancellationDto> _processCancellationEventStream =
      new Subject<ProcessCancellationDto>();

    public IObservable<ProcessCancellationDto> ProcessCancellationEvents =>
      _processCancellationEventStream.AsObservable();

    private bool _isProcessing = false;

    public void TriggerProcessStart(ProcessStartDto processStart)
    {
      _isProcessing = true;
      _processStartEventStream.OnNext(processStart);
    }

    public void TriggerProcessFinish(ProcessFinishDto processFinish)
    {
      _isProcessing = false;
      _processFinishEventStream.OnNext(processFinish);
    }

    public void TriggerProcessCancellation()
    {
      if (!_isProcessing)
      {
        _processCancellationEventStream.OnNext(new ProcessCancellationDto());
      }
    }

    // Implement IObservable<ProcessCancellationDto> for backward compatibility
    public IDisposable Subscribe(IObserver<ProcessCancellationDto> observer)
    {
      return _processCancellationEventStream.Subscribe(observer);
    }
  }
}