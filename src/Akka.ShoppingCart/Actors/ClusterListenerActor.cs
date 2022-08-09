// -----------------------------------------------------------------------
//  <copyright file="ClusterListenerActor.cs" company="Akka.NET Project">
//      Copyright (C) 2013-2022 .NET Foundation <https://github.com/akkadotnet/akka.net>
//  </copyright>
// -----------------------------------------------------------------------

using Akka.Cluster;

namespace Akka.ShoppingCart.Actors;

public sealed class RegisterCallback
{
    public RegisterCallback(Func<Task> callback)
    {
        Callback = callback;
    }

    public Func<Task> Callback { get; }
}

public class ClusterListenerActor: ReceiveActor
{
    private Func<Task>? _callback;
    
    public ClusterListenerActor()
    {
        ReceiveAsync<ClusterEvent.MemberStatusChange>(async _ =>
        {
            if(_callback != null)
                await _callback();
        });

        Receive<RegisterCallback>(message =>
        {
            _callback = message.Callback;
        });
    }

    protected override void PreStart()
    {
        base.PreStart();
        var cluster = Cluster.Cluster.Get(Context.System);
        cluster.Subscribe(Self, ClusterEvent.SubscriptionInitialStateMode.InitialStateAsEvents, typeof(ClusterEvent.MemberStatusChange));
    }
}