using Sample.Domain;

namespace Serialization.Tests;

public class CsvSerializerTests
{
    [Fact]
    public void RoundTrip_preserves_all_scalar_members()
    {
        var value = new Measurement
        {
            Enabled = true,
            OptionalEnabled = false,
            Count = -42,
            OptionalCount = 7,
            Total = 1234567890123L,
            OptionalTotal = -9L,
            Ratio = 0.1,
            OptionalRatio = 2.5,
            Amount = 12345.6789m,
            OptionalAmount = -0.005m,
            Label = "sensor-1",
            Note = "over, the \"hill\"",
        };

        var serialized = MeasurementCsvSerializer.Serialize(value);
        var back = MeasurementCsvSerializer.Deserialize(serialized);
        var reserialized = MeasurementCsvSerializer.Serialize(back);

        Assert.Equal(value, back);
        Assert.Equal(serialized, reserialized);
    }

    [Fact]
    public void RoundTrip_preserves_null_and_empty_members()
    {
        var value = new Measurement
        {
            Enabled = false,
            Count = 0,
            Total = 0,
            Ratio = 0,
            Amount = 0m,
            Label = "",
        };

        var back = MeasurementCsvSerializer.Deserialize(MeasurementCsvSerializer.Serialize(value));

        Assert.Equal(value, back);
        Assert.Equal("", back.Label);
        Assert.Null(back.Note);
        Assert.Null(back.OptionalEnabled);
        Assert.Null(back.OptionalRatio);
    }

    [Fact]
    public void RoundTrip_preserves_numeric_boundaries()
    {
        var value = new Measurement
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

        var serialized = MeasurementCsvSerializer.Serialize(value);
        var back = MeasurementCsvSerializer.Deserialize(serialized);
        var reserialized = MeasurementCsvSerializer.Serialize(back);

        Assert.Equal(value, back);
        Assert.Equal(serialized, reserialized);
    }

    [Fact]
    public void Serialize_emits_header_row_then_value_row()
    {
        var csv = MeasurementCsvSerializer.Serialize(new Measurement
        {
            Enabled = true, Count = 1, Total = 2, Ratio = 3, Amount = 4m, Label = "l",
        });

        var lines = csv.Split('\n');
        Assert.Equal(
            "enabled,optionalEnabled,count,optionalCount,total,optionalTotal,ratio,optionalRatio,amount,optionalAmount,label,note",
            lines[0]);
        Assert.Equal("true,,1,,2,,3,,4,,l,", lines[1]);
    }

    [Fact]
    public void Serialize_quotes_fields_containing_commas_quotes_and_newlines()
    {
        var csv = MeasurementCsvSerializer.Serialize(new Measurement
        {
            Enabled = true, Count = 1, Total = 2, Ratio = 3, Amount = 4m,
            Label = "a,\"b\",c",
            Note = "line1\nline2",
        });

        Assert.Contains("\"a,\"\"b\"\",c\"", csv);
        Assert.Contains("\"line1\nline2\"", csv);
    }

    [Fact]
    public void RoundTrip_handles_quoted_special_characters()
    {
        var value = new Measurement
        {
            Enabled = true, Count = 1, Total = 2, Ratio = 3, Amount = 4m,
            Label = "a,\"b\",c",
            Note = "line1\nline2",
        };

        var back = MeasurementCsvSerializer.Deserialize(MeasurementCsvSerializer.Serialize(value));

        Assert.Equal(value, back);
    }

    [Fact]
    public void Serialize_quotes_empty_string_but_leaves_null_empty()
    {
        var csv = MeasurementCsvSerializer.Serialize(new Measurement
        {
            Enabled = true, Count = 1, Total = 2, Ratio = 3, Amount = 4m,
            Label = "",
            Note = null,
        });

        Assert.Contains(",\"\",", csv);
    }

    [Fact]
    public void Golden_output_matches_exactly()
    {
        var expected = File.ReadAllText(
            Path.Combine(AppContext.BaseDirectory, "Fixtures", "measurement.csv")).TrimEnd('\r', '\n');
        var actual = MeasurementCsvSerializer.Serialize(new Measurement
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
            Label = "sensor,1",
            Note = "ready",
        });

        Assert.Equal(expected, actual);
    }
}
