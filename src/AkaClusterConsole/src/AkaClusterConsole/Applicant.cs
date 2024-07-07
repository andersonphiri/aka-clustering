using Akka.Persistence;

namespace AkaClusterConsole;

public class Applicant : UntypedPersistentActor
{
    public override string PersistenceId { get; }
    protected override void OnCommand(object message)
    {
        throw new NotImplementedException();
    }

    protected override void OnRecover(object message)
    {
        throw new NotImplementedException();
    }
}