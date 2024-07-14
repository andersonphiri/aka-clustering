using System.Diagnostics.CodeAnalysis;
using Akka.Actor;
using Akka.Serialization;

namespace AkaClusterConsole.Actors.Contracts;

public abstract class CustomAkkaProtobufMessageSerializer<TMessageSerializer> : SerializerWithStringManifest 
where TMessageSerializer : CustomAkkaProtobufMessageSerializer<TMessageSerializer>
{
    /// <summary>
    ///  for <paramref name="identifier"/> use a unique value greater than 100 as 0-100 is akka system reserved 
    /// </summary>
    /// <param name="identifier"></param>
    /// <param name="system"></param>
    protected CustomAkkaProtobufMessageSerializer(int identifier, ExtendedActorSystem system) : base(system)
    {
        Identifier = identifier;
    } 
    
    private static readonly Dictionary<Type,string> manifest = new ();
    private static readonly Dictionary<Type, Func< object, byte[]>> serializers = new();
    private static readonly Dictionary<string, Func<byte[], object>> deserializers = new();
    public override int Identifier { get; }

    protected static void Add<T>(string manifestKey) where T : class
    {
        if (!manifest.TryAdd(typeof(T), manifestKey))
        {
            throw new ArgumentException($"Message serializer manifest already added: {manifestKey} -> {typeof(T)}");
        }

        if (!serializers.TryAdd(typeof(T), Serialize))
        {
            throw new ArgumentException($"Message serializer Type already added: {manifestKey} -> {typeof(T)}");

        }

        deserializers[manifestKey] = Deserialize<T>;
    }

    public static bool TryGetManifest<T>([MaybeNullWhen(false)] out string manifestKey)
    {
        return manifest.TryGetValue(typeof(T), out manifestKey);
    } 
    
    public static bool TryGetManifest( Type @type,[MaybeNullWhen(false)] out string manifestKey)
    {
        return manifest.TryGetValue(@type, out manifestKey);
    }
    public static object Deserialize<T>(byte[] bytes) where T : class
    {
        Console.WriteLine($"Message type {typeof(T)} deserialized");
        return ProtoBuf.Serializer.Deserialize<T>(bytes.AsSpan());
    } 
    
    public static bool TryGetSerializer(Type objectType, [MaybeNullWhen(false)] out Func<object, byte[]> serializer)
        => serializers.TryGetValue(objectType, out serializer);
    public static bool TryGetSerializer<T>([MaybeNullWhen(false)] out Func<object, byte[]> serializer)
        => serializers.TryGetValue(typeof(T), out serializer);

    public static bool TryGetDeserializer(string manifestKey, [MaybeNullWhen(false)] out Func<byte[], object> deserializer)
        => deserializers.TryGetValue(manifestKey, out deserializer);


    public static byte[] Serialize(object @object)
    {
        Console.WriteLine($"Message type {@object.GetType()} serialized");
        using (MemoryStream ms = new MemoryStream())
        {
            ProtoBuf.Serializer.Serialize(ms,@object);
            return ms.ToArray();
        }
    } 
    
    // binary overides
    public static string GetManifest<T>()
    {
        if (manifest.TryGetValue(typeof(T), out var key))
            return key;
        throw new ArgumentOutOfRangeException("{T}", $"Unsupported message type [{typeof(T)}]");
    }

    public override string Manifest(object o)
    {
        if (TryGetManifest(o.GetType(), out var key))
            return key;
        throw new ArgumentOutOfRangeException(nameof(o), $"Unsupported message type [{o.GetType()}]");
    }

    public override object FromBinary(byte[] bytes, string manifest)
    {
        if (TryGetDeserializer(manifest, out var deserializer))
            return deserializer(bytes);
        throw new ArgumentOutOfRangeException(nameof(manifest), $"Unsupported message manifest [{manifest}]");
    }

    public override byte[] ToBinary(object obj)
    {
        if (TryGetSerializer(obj.GetType(), out var serializer))
            return serializer(obj);
        throw new ArgumentOutOfRangeException(nameof(obj), $"Unsupported message type [{obj.GetType()}]");
    }

    
}