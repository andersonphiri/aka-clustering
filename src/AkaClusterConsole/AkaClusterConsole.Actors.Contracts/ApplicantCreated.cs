using System.ComponentModel.DataAnnotations;
using ProtoBuf;

namespace AkaClusterConsole.Actors.Contracts;

[ProtoContract]
public sealed record ApplicantCreated : IActorContractProtocolMember
{
    [ProtoMember(1)] [Required] public Guid Id { get; init; } 
    [ProtoMember(2)] [Required] public DateTimeOffset When { get; init; } 
    
}