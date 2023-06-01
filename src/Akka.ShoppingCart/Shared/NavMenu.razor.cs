// -----------------------------------------------------------------------
//  <copyright file="NavMenu.razor.cs" company="Petabridge, LLC">
//      Copyright (C) 2015-2023 Petabridge, LLC <https://petabridge.com>
//      Copyright (c) Microsoft. All rights reserved.
//      Licensed under the MIT License.
//  </copyright>
// -----------------------------------------------------------------------

namespace Akka.ShoppingCart.Shared;

public partial class NavMenu: IDisposable
{
    private int _count = 0;
    private int _clusterCount = 0;

    [Inject]
    public ComponentStateChangedObserver Observer { get; set; } = null!;

    [Inject] 
    public ShoppingCartService Cart { get; set; } = null!;
    
    [Inject] 
    public ClusterStateBroadcastService ClusterStateListener { get; set; } = null!;


    protected override async Task OnInitializedAsync()
    {
        Observer.OnStateChanged += UpdateCountAsync;
        ClusterStateListener.OnClusterStateChanged += UpdateClusterCountAsync;
        _clusterCount = ClusterStateListener.ConnectedCluster;
        
        await UpdateCountAsync();
    }

    private Task UpdateCountAsync() =>
        InvokeAsync(async () =>
        {
            _count = await Cart.GetCartCountAsync();
            StateHasChanged();
        });

    private Task UpdateClusterCountAsync()=>
        InvokeAsync(() =>
        {
            _clusterCount = ClusterStateListener.ConnectedCluster;
            StateHasChanged();
        });

    public void Dispose()
    {
        Observer.OnStateChanged -= UpdateCountAsync;
        ClusterStateListener.OnClusterStateChanged -= UpdateClusterCountAsync;
    }
}