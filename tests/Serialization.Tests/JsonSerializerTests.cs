using Sample.Domain;

namespace Serialization.Tests;

public class JsonSerializerTests
{
    [Fact]
    public void Flat_round_trip_is_structurally_stable()
    {
        var value = BoundaryMeasurement();

        var serialized = MeasurementJsonSerializer.Serialize(value);
        var deserialized = MeasurementJsonSerializer.Deserialize(serialized);
        var reserialized = MeasurementJsonSerializer.Serialize(deserialized);

        Assert.Equal(value, deserialized);
        Assert.True(JsonStructuralComparer.Equivalent(serialized, reserialized));
    }

    [Fact]
    public void Nested_round_trip_is_structurally_stable()
    {
        var value = new WeatherStation
        {
            Id = "station-1",
            Name = null,
            Location = new Coordinates
            {
                Latitude = 45.5017,
                Longitude = -73.5673,
                Altitude = null,
            },
        };

        var serialized = WeatherStationJsonSerializer.Serialize(value);
        var deserialized = WeatherStationJsonSerializer.Deserialize(serialized);
        var reserialized = WeatherStationJsonSerializer.Serialize(deserialized);

        Assert.Equal(value, deserialized);
        Assert.True(JsonStructuralComparer.Equivalent(serialized, reserialized));
    }

    [Fact]
    public void Structural_comparison_ignores_formatting_and_property_order()
    {
        const string compact = "{\"name\":\"station\",\"location\":{\"latitude\":1,\"longitude\":2}}";
        const string formatted = """
            {
              "location": {
                "longitude": 2.0,
                "latitude": 1.0
              },
              "name": "station"
            }
            """;

        Assert.True(JsonStructuralComparer.Equivalent(compact, formatted));
    }

    [Fact]
    public void Golden_output_matches_exactly()
    {
        var expected = ReadFixture("measurement.json");
        var actual = MeasurementJsonSerializer.Serialize(new Measurement
        {
            Enabled = true,
            OptionalEnabled = null,
            Count = 12,
            OptionalCount = -3,
            Total = 4000000000,
            OptionalTotal = null,
            Ratio = 0.5,
            OptionalRatio = null,
            Amount = 19.95m,
            OptionalAmount = null,
            Label = "sensor-1",
            Note = "ready",
        });

        Assert.Equal(expected, actual);
    }

    private static string ReadFixture(string name) =>
        File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "Fixtures", name)).TrimEnd('\r', '\n');

    private static Measurement BoundaryMeasurement() => new()
    {
        Enabled = false,
        OptionalEnabled = true,
        Count = int.MinValue,
        OptionalCount = int.MaxValue,
        Total = long.MinValue,
        OptionalTotal = long.MaxValue,
        Ratio = double.MinValue,
        OptionalRatio = double.MaxValue,
        Amount = decimal.MinValue,
        OptionalAmount = decimal.MaxValue,
        Label = "",
        Note = null,
    };
}
