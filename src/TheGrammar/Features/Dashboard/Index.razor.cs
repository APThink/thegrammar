using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using TheGrammar.Database;
using MudBlazor;
using TheGrammar.Features.HotKeys.Events;

namespace TheGrammar.Features.Dashboard;

public partial class Index : IDisposable
{
    [Inject] public IProcessInputEventService IProcessInputEventService { get; set; } = null!;
    [Inject] public IDbContextFactory<ApplicationDbContext> DbContextFactory { get; set; } = null!;

    private IDisposable? subscription;

    private List<ProcessFinishDto> messages = new();
    private MudTextField<string>? multilineReference;

    private int requestCount;

    protected override async Task OnInitializedAsync()
    {
        subscription = IProcessInputEventService.ProcessFinishEvents.Subscribe(OnEventReceived);
        await CountRequestAsync();
        await GetLastTenRequestsAsync();
    }

    private void OnEventReceived(ProcessFinishDto processFinishDto)
    {
        messages.Insert(0, processFinishDto);

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
            messages.Insert(0, new ProcessFinishDto(OriginalText: request.RequestText, ModifiedText: request.ResponseText, ChatVersion: request.ChatVersion));
        }
    }

    private async Task CopyTextToClipboard()
    {
        if(string.IsNullOrWhiteSpace(multilineReference?.Value))
        {
            return;
        }

        await multilineReference.SelectAsync();
        Clipboard.SetText(multilineReference.Value);
    }

    public void Dispose()
    {
        subscription?.Dispose();
    }
}
