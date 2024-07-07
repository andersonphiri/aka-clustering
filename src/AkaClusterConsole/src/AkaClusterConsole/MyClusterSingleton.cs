using Akka.Actor;
using Akka.Cluster.Tools.Singleton;
using Akka.Persistence;

namespace AkaClusterConsole;

public class MyClusterSingleton : UntypedPersistentActor
{
    public const string Id = "{8E60A698-9D05-41DC-AB2D-340B2303A437}";
    public const string Name = "OurSingleton";
    public static Props Props { get; } = Props.Create(() => new MyClusterSingleton());
    public override string PersistenceId { get; }
    protected override void OnCommand(object message)
    {
        throw new NotImplementedException();
    }

    void TalkToSingleTon()
    { 
        
        var _actorSystem = Context.System;
        var priceInitiator = _actorSystem.ActorOf(ClusterSingletonManager.Props(
            MyClusterSingleton.Props,
            ClusterSingletonManagerSettings.Create(_actorSystem).WithRole("pricing-engine")
        ), "priceInitiator");
    }
    protected override void OnRecover(object message)
    {
        throw new NotImplementedException();
    }
}