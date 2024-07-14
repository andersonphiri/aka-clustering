using System.ComponentModel.DataAnnotations;
using Akka.Actor;
using ProtoBuf;

namespace AkaClusterConsole.Actors.Contracts;

public interface IActorContractProtocolMember
{
    
}

[ProtoContract]
public sealed record ApplicantCreated : IActorContractProtocolMember
{
    [ProtoMember(1)] [Required] public Guid Id { get; init; } 
    [ProtoMember(2)] [Required] public DateTimeOffset When { get; init; } 
    
}

[ProtoContract]
public sealed record ApplicantReloaded : IActorContractProtocolMember
{
    [ProtoMember(1)] [Required] public Guid Id { get; init; } 
}

public class ActorContractMessageSerializer : CustomAkkaProtobufMessageSerializer<ActorContractMessageSerializer>
{
    public const int Id = 2000;
    public ActorContractMessageSerializer(ExtendedActorSystem system) : base(Id, system)
    {
    }

    static ActorContractMessageSerializer()
    {
        Add<ApplicantCreated>("ac");
        Add<ApplicantReloaded>("ar");
    }
}