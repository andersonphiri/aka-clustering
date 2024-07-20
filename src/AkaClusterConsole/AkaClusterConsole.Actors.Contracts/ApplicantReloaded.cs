using System.ComponentModel.DataAnnotations;
using ProtoBuf;

namespace AkaClusterConsole.Actors.Contracts;

[ProtoContract]
public sealed record ApplicantReloaded : IActorContractProtocolMember
{
    [ProtoMember(1)] [Required] public Guid Id { get; init; } 
}