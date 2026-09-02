using System.Text.Json;

namespace Serialization.Tests;

internal static class JsonStructuralComparer
{
    internal static bool Equivalent(string expected, string actual)
    {
        using var expectedDocument = JsonDocument.Parse(expected);
        using var actualDocument = JsonDocument.Parse(actual);
        return Equivalent(expectedDocument.RootElement, actualDocument.RootElement);
    }

    private static bool Equivalent(JsonElement expected, JsonElement actual)
    {
        if (expected.ValueKind != actual.ValueKind)
            return false;

        switch (expected.ValueKind)
        {
            case JsonValueKind.Object:
                var expectedProperties = expected.EnumerateObject().ToArray();
                var actualProperties = actual.EnumerateObject().ToArray();
                return expectedProperties.Length == actualProperties.Length
                    && expectedProperties.All(property =>
                        actual.TryGetProperty(property.Name, out var actualValue)
                        && Equivalent(property.Value, actualValue));
            case JsonValueKind.Array:
                var expectedItems = expected.EnumerateArray().ToArray();
                var actualItems = actual.EnumerateArray().ToArray();
                return expectedItems.Length == actualItems.Length
                    && expectedItems.Zip(actualItems).All(pair => Equivalent(pair.First, pair.Second));
            case JsonValueKind.String:
                return expected.GetString() == actual.GetString();
            case JsonValueKind.Number:
                return expected.TryGetDecimal(out var expectedDecimal)
                    && actual.TryGetDecimal(out var actualDecimal)
                        ? expectedDecimal == actualDecimal
                        : expected.GetDouble().Equals(actual.GetDouble());
            case JsonValueKind.True:
            case JsonValueKind.False:
                return expected.GetBoolean() == actual.GetBoolean();
            case JsonValueKind.Null:
                return true;
            default:
                return expected.GetRawText() == actual.GetRawText();
        }
    }
}
