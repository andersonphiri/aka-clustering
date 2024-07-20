using Akka.Hosting;

namespace AkaClusterConsole.Actors.Contracts;

public static class ProtobufAkkaSerializerExtension
{
    public static AkkaConfigurationBuilder WithCustomProtobufSerialization(this AkkaConfigurationBuilder builder)
    { 
        //  builder.Cr
        return builder.WithCustomSerializer("device-events-messages", [typeof(IActorContractProtocolMember)],
            system => new ActorContractMessageSerializer(system)
        ).WithCustomSerializer("protobuf-dto-serder", [typeof(Google.Protobuf.IMessage<>)], 
                system => new DtoProtobufContractSerializer(system) )
            ;
        
        
    }
}