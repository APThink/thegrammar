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

    private List<PushDto> messages = new();
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
        messages.Insert(0, pushDto);

        // Remove the oldest element if there are more than 10 elements in the list
        if (messages.Count > 10)
        {
            messages.RemoveAt(10); // Removes the 11th element, keeping the list size to 10
        }

        // Increment the request count and invoke the UI update
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
            messages.Insert(0, new PushDto
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
