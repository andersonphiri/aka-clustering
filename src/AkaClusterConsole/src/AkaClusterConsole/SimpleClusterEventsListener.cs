using Akka.Actor;
using Akka.Cluster;
using Akka.Cluster.Tools.PublishSubscribe;
using Akka.Event;

namespace AkaClusterConsole;

public class SimpleClusterEventsListener : UntypedActor, IWithTimers
{
    public static class Messages
    {
        public record ReminderToSendToTopic();
    }
    protected ILoggingAdapter _logger = Context.GetLogger();
    protected Akka.Cluster.Cluster Cluster = Akka.Cluster.Cluster.Get(Context.System);
    private IActorRef mediator = null;
    private int count = 0;
    public SimpleClusterEventsListener()
    {
        mediator = DistributedPubSub.Get(Context.System).Mediator;
    }
    protected override void PreStart()
    {
        Cluster.Subscribe(Self, ClusterEvent.InitialStateAsEvents, typeof(ClusterEvent.IMemberEvent));
        Timers.StartPeriodicTimer("sendtotopicreminder", new Messages.ReminderToSendToTopic(),TimeSpan.FromSeconds(2), 
            TimeSpan.FromMilliseconds(2000));
        base.PreStart();
    }

    protected override void OnReceive(object message)
    {
        switch (message)
        {
            case ClusterEvent.MemberUp up:
                _logger.Info("from listener, member of the cluster is up: {0}", up.Member);
                break;
            case ClusterEvent.MemberJoined mj:
                _logger.Info("from listener, member has joiuned: {0}", mj.Member);
                break;
            case ClusterEvent.MemberDowned md:
                _logger.Info("from listener, member has has been downed: {0}", md.Member);
                break;
            case Messages.ReminderToSendToTopic _:
                mediator.Tell(new Publish("frames",new PictureFrame(DateTimeOffset.Now, Guid.NewGuid())));
                count++;
                if (count > 10)
                {
                    Timers.CancelAll();
                }
                break;
        }
    }

    public ITimerScheduler Timers { get; set; }
}

public record PictureFrame(DateTimeOffset When, Guid Id);

public class DistributedPubSubSubscriber : ReceiveActor
{ 
    protected ILoggingAdapter _logger = Context.GetLogger();
    public DistributedPubSubSubscriber()
    {
        var mediator = DistributedPubSub.Get(Context.System).Mediator;
        mediator.Tell(new Subscribe("frames", Self));
        Receive<SubscribeAck>(ack => { _logger.Info("my request to subscribe has been consfirmed: {0}", ack); });
        Receive<PictureFrame>(pf =>
        {
            _logger.Info("received picture frame from topic: {0}", pf);
        });

    }
}