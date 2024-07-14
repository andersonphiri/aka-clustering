using Akka.Hosting;

namespace AkaClusterConsole.Actors.Contracts;

public static class ProtobufAkkaSerializerExtension
{
    public static AkkaConfigurationBuilder AddCustomProtobufSerialization(this AkkaConfigurationBuilder builder)
    { 
        //  builder.Cr
        return builder.WithCustomSerializer("device-events-messages", [typeof(IActorContractProtocolMember)],
            system => new ActorContractMessageSerializer(system)
        );
        
        
    }
}