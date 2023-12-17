using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace TheGrammar.Services;

public class PushDto
{
    private string? _text;

    public string? Text
    {
        get => _text;
        set => _text = value != null && value.Length > 45 ? value[..45] : value;
    }

    public string? ModifiedText { get; set; }
}

public class PushService
{
    private readonly ISubject<PushDto> _eventStream = new Subject<PushDto>();

    public IObservable<PushDto> Events => _eventStream.AsObservable();

    public void TriggerEvent(PushDto pushDto)
    {
        _eventStream.OnNext(pushDto);
    }
}