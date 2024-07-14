### This library uses recomended serilization for akka.Net actor messages across nodes

- for local clr message it is not necessary to use serialization
- for cross actor remote when using (remote or cluster) providers
- the implemented serializer uses Protobuf based serializer from https://github.com/protobuf-net/protobuf-net
- this allows us to use imutable and yet brotobuf serialized messages
- articles used: 
  - https://getakka.net/articles/serialization/serialization.html 
  - https://dev.to/rafalpiotrowski/protobuf-message-serialization-in-akkanet-using-protobuf-net-970 
  - the last article sample repo is found at: 
    - https://github.com/rafalpiotrowski/AkkaProtoBufMessageSerialization/tree/main  


#### Designing domain messages 
  - https://petabridge.com/blog/how-to-design-akkadotnet-domain-messages/ 