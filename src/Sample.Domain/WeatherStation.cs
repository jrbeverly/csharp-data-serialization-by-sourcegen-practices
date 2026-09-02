using Serialization.Abstractions;

namespace Sample.Domain
{
    [GenerateSerializers(SerializationFormat.Json)]
    public sealed record WeatherStation
    {
        [SerializeMember("id", 0)] public required string Id { get; init; }
        [SerializeMember("name", 1)] public string? Name { get; init; }
        [SerializeMember("location", 2)] public required Coordinates Location { get; init; }
    }
}
