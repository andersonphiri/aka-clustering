using Akka.Actor;
using Akka.DistributedData;

namespace AkaClusterConsole;

public record Data() : IReplicatedData
{
    public IReplicatedData Merge(IReplicatedData other)
    {
        throw new NotImplementedException();
    }
}

public record StringKey() : IKey<Data>
{
    public string Id { get; }
}

public class DDataActor : UntypedActor
{
    protected readonly IActorRef _subscriber = null;
    protected override void PreStart()
    {
        // var _cluster = Akka.Cluster.Cluster.Get(Context.System);
        // var  settings = ReplicatorSettings.Create(Context.System).WithGossipInterval(TimeSpan.FromSeconds(1)).WithMaxDeltaElements(10);
        // var props = Replicator.Props(settings);
        // var _replicator = Context.System.ActorOf(props, "replicator");
        // _replicator.Tell(Dsl.Subscribe(new StringKey(),_subscriber));
        base.PreStart();
        // 1:58 duck://player/8PenRoEjZKc 
    }

    protected override void OnReceive(object message)
    {
        
    }
}