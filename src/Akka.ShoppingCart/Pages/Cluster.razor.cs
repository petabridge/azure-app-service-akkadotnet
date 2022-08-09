// -----------------------------------------------------------------------
//  <copyright file="Cluster.razor.cs" company="Akka.NET Project">
//      Copyright (C) 2013-2022 .NET Foundation <https://github.com/akkadotnet/akka.net>
//  </copyright>
// -----------------------------------------------------------------------

using Akka.Cluster;

namespace Akka.ShoppingCart.Pages;

public partial class Cluster: IDisposable
{
    [Inject] 
    public ClusterStateBroadcastService BroadcastService { get; set; } = null!;

    private ClusterEvent.CurrentClusterState? _state;

    protected override Task OnInitializedAsync() => GetClusterStatusAsync();
    
    private Task GetClusterStatusAsync() =>
        InvokeAsync(() =>
        {
            BroadcastService.OnClusterStateChanged += OnClusterStateChanged;
            _state = BroadcastService.CurrentState;
            StateHasChanged();
        });

    private async Task OnClusterStateChanged()
    {
        _state = BroadcastService.CurrentState;
        await InvokeAsync(StateHasChanged);
    }

    private string MemberRowStyle(Member member, int index)
        => member.Status switch
        {
            MemberStatus.Up => "background-color: lightgreen",
            MemberStatus.Exiting => "background-color: lightyellow",
            MemberStatus.Leaving => "background-color: lightyellow",
            MemberStatus.Joining => "background-color: lightblue",
            MemberStatus.WeaklyUp => "background-color: lightblue",
            _ => "background-color: lightcoral",
        };

    public void Dispose()
    {
        BroadcastService.OnClusterStateChanged -= OnClusterStateChanged;
    }
}