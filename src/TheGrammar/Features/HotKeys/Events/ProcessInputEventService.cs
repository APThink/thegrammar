using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace TheGrammar.Features.HotKeys.Events
{
  public record ProcessStartDto(string Input, string Prompt);
  public record ProcessFinishDto(string OriginalText, string ModifiedText, string? ModelKey);
  public record ProcessCancellationDto();
  public record ProcessErrorDto(string Message);

  public interface IProcessInputEventService
  {
    IObservable<ProcessStartDto> ProcessStartEvents { get; }
    void TriggerProcessStart(ProcessStartDto processStart);

    IObservable<ProcessFinishDto> ProcessFinishEvents { get; }
    void TriggerProcessFinish(ProcessFinishDto processFinish);

    IObservable<ProcessCancellationDto> ProcessCancellationEvents { get; }
    void TriggerProcessCancellation();

    IObservable<ProcessErrorDto> ProcessErrorEvents { get; }
    void TriggerProcessError(ProcessErrorDto processError);
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

    private readonly ISubject<ProcessErrorDto> _processErrorEventStream = new Subject<ProcessErrorDto>();
    public IObservable<ProcessErrorDto> ProcessErrorEvents => _processErrorEventStream.AsObservable();

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

    public void TriggerProcessError(ProcessErrorDto processError)
    {
      _isProcessing = false;
      _processErrorEventStream.OnNext(processError);
    }

    // Implement IObservable<ProcessCancellationDto> for backward compatibility
    public IDisposable Subscribe(IObserver<ProcessCancellationDto> observer)
    {
      return _processCancellationEventStream.Subscribe(observer);
    }
  }
}