using AkaClusterConsole.Dtos.Message;
using Akka.Actor;
using Google.Protobuf;

namespace AkaClusterConsole.Actors.Contracts;

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


public class DtoProtobufContractSerializer : CustomAkkaProtobufMessageSerializer<DtoProtobufContractSerializer>
{
    public const int Id = 2020;
    public DtoProtobufContractSerializer(ExtendedActorSystem system) : base(Id, system)
    {
        
    } 
    static DtoProtobufContractSerializer()
    {
        Add<IntermediateFrame>("imfr");
        Add<SdkStatus>("sdkst");
    }
    
    
}