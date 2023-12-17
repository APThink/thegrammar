using TheGrammar.Services;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using Microsoft.EntityFrameworkCore;
using TheGrammar.Data;
namespace TheGrammar.View.Pages;

public partial class Index : IDisposable
{
    [Inject] public PushService PushService { get; set; } = null!;
    [Inject] public IDbContextFactory<AppDbContext> DbContextFactory { get; set; } = null!;


    private IDisposable? subscription;

    private Stack<PushDto> messages = new();
    private MudTextField<string>? multilineReference;

    private int requestCount;
    
    protected override async Task OnInitializedAsync()
    {
        subscription = PushService.Events.Subscribe(OnEventReceived);
        await CountRequestAsync();
        await GetLastTenRequestsAsync();
    }

    private void OnEventReceived(PushDto pushDto)
    {
        messages.Push(pushDto);

        if (messages.Count > 10)
        {
            messages.Pop();
        }

        requestCount += 1;
        InvokeAsync(StateHasChanged);
    }

    private async Task CountRequestAsync()
    {
        using var context = DbContextFactory.CreateDbContext();
        requestCount = await context.Requests.CountAsync();
    }

    private async Task GetLastTenRequestsAsync()
    {
        using var context = DbContextFactory.CreateDbContext();
        var requests = await context.Requests.OrderByDescending(x => x.Id).Take(10).ToListAsync();

        requests.Reverse();

        foreach (var request in requests)
        {
            messages.Push(new PushDto
            {
                Text = request.RequestText,
                ModifiedText = request.ResponseText
            });
        }
    }


    public void Dispose()
    {
        subscription?.Dispose();
    }
}
