using Serialization.Abstractions;

namespace Sample.Domain
{
    [GenerateSerializers(SerializationFormat.Json)]
    public sealed record Coordinates
    {
        [SerializeMember("latitude", 0)] public required double Latitude { get; init; }
        [SerializeMember("longitude", 1)] public required double Longitude { get; init; }
        [SerializeMember("altitude", 2)] public double? Altitude { get; init; }
    }
}
