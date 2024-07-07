using Akka.Cluster.Sharding;

namespace AkaClusterConsole;

public interface IWithStockId
{
    public string StockId { get;  }
}

public interface IConfirmableMessageEnvelope<T>
{
    public T Message { get; }
}
public class ApplicantShardMessageRouter : HashCodeMessageExtractor
{
    public ApplicantShardMessageRouter(int maxNumberOfShards) : base(maxNumberOfShards)
    { 
        // Rule of thumb = Max Shards = Nodes * 10
    }

    public override string EntityId(object message)
    {
        switch (message)
        {
            case IWithStockId si:
                return si.StockId;
                break;
            case IConfirmableMessageEnvelope<IWithStockId> env:
                return env.Message.StockId;
                break;
            default: return null;
                
        }
    }
}