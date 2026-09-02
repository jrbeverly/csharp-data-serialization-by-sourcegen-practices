# C# Data Serialization by Source Generation

Generates reflection-free JSON and CSV serializers from one annotated C# domain model.

```csharp
[GenerateSerializers(SerializationFormat.Json | SerializationFormat.Csv)]
public sealed record Measurement
{
    [SerializeMember("enabled", 0)] public required bool Enabled { get; init; }
    [SerializeMember("optionalEnabled", 1)] public bool? OptionalEnabled { get; init; }
    [SerializeMember("count", 2)] public required int Count { get; init; }
    // ...
    [SerializeMember("note", 11)] public string? Note { get; init; }
}
```

Generated `MeasurementCsvSerializer` (see `evidence/`):

```csharp
public static string Serialize(Measurement value)
{
    var sb = new System.Text.StringBuilder();
    sb.Append(Header);
    sb.Append('\n');
    sb.Append(value.Enabled ? "true" : "false");
    sb.Append(',');
    sb.Append(value.OptionalEnabled.HasValue ? (value.OptionalEnabled.Value ? "true" : "false") : "");
    sb.Append(',');
    sb.Append(value.Count.ToString(System.Globalization.CultureInfo.InvariantCulture));
    // ...
    sb.Append(value.Note is null ? "" : EscapeField(value.Note));
    return sb.ToString();
}
```

```sh
make build
make test
```

## Notes

- One canonical member descriptor supplies both serialization backends.
- Generated JSON code drives `Utf8JsonReader` and `Utf8JsonWriter` directly.
- CSV distinguishes null from an empty string and round-trips quoted commas, quotes, CR, and LF.
- Nested CSV members produce the error diagnostic `SER001` and suppress serializer emission.
