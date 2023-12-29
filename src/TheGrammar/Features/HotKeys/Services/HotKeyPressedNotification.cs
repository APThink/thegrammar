using System.Reactive.Linq;
using System.Reactive.Subjects;
using TheGrammar.Domain;

namespace TheGrammar.Features.HotKeys.Services;

public class HotKeyPressedNotification
{
    private readonly ISubject<HotKeyPressedNotificationDto> _eventStream = new Subject<HotKeyPressedNotificationDto>();

    public IObservable<HotKeyPressedNotificationDto> Events => _eventStream.AsObservable();

    public void TriggerEvent(HotKeyPressedNotificationDto pushDto)
    {
        _eventStream.OnNext(pushDto);
    }
}

public class HotKeyPressedNotificationDto
{
    private string? _text;

    public string? OriginalText
    {
        get => _text;
        set => _text = value != null && value.Length > 85 ? value[..85] : value;
    }

    public string? ModifiedText { get; set; }
    public ChatVersion? ChatVersion { get; set; }
}