// -----------------------------------------------------------------------
//  <copyright file="ClusterStateBroadcastService.cs" company="Akka.NET Project">
//      Copyright (C) 2013-2022 .NET Foundation <https://github.com/akkadotnet/akka.net>
//  </copyright>
// -----------------------------------------------------------------------

using Akka.Cluster;

namespace Akka.ShoppingCart.Services;

public sealed class ClusterStateBroadcastService
{
    private readonly Cluster.Cluster _cluster;

    public ClusterStateBroadcastService(ActorSystem system)
    {
        _cluster = Cluster.Cluster.Get(system);
        var resolver = DependencyResolver.For(system);
        var registry = resolver.Resolver.GetService<ActorRegistry>();
        var clusterListener = registry.Get<RegistryKey.ClusterStateListener>();
        clusterListener.Tell(new RegisterCallback(OnClusterStateChangedCallback));
    }

    public event Func<Task>? OnClusterStateChanged;
    
    public ClusterEvent.CurrentClusterState CurrentState => _cluster.State;

    public int ConnectedCluster => _cluster.State.Members.Count(
        m => m.Status is MemberStatus.Joining or MemberStatus.WeaklyUp or MemberStatus.Up);

    private Task OnClusterStateChangedCallback()
        => OnClusterStateChanged?.Invoke() ?? Task.CompletedTask;
}