// -----------------------------------------------------------------------
//  <copyright file="ClusterListenerActor.cs" company="Petabridge, LLC">
//      Copyright (C) 2015-2023 Petabridge, LLC <https://petabridge.com>
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