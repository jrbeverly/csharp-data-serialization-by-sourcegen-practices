using Serialization.Abstractions;

namespace Sample.Domain
{
    [GenerateSerializers(SerializationFormat.Json | SerializationFormat.Csv)]
    public sealed record Measurement
    {
        [SerializeMember("enabled", 0)] public required bool Enabled { get; init; }
        [SerializeMember("optionalEnabled", 1)] public bool? OptionalEnabled { get; init; }
        [SerializeMember("count", 2)] public required int Count { get; init; }
        [SerializeMember("optionalCount", 3)] public int? OptionalCount { get; init; }
        [SerializeMember("total", 4)] public required long Total { get; init; }
        [SerializeMember("optionalTotal", 5)] public long? OptionalTotal { get; init; }
        [SerializeMember("ratio", 6)] public required double Ratio { get; init; }
        [SerializeMember("optionalRatio", 7)] public double? OptionalRatio { get; init; }
        [SerializeMember("amount", 8)] public required decimal Amount { get; init; }
        [SerializeMember("optionalAmount", 9)] public decimal? OptionalAmount { get; init; }
        [SerializeMember("label", 10)] public required string Label { get; init; }
        [SerializeMember("note", 11)] public string? Note { get; init; }
    }
}
