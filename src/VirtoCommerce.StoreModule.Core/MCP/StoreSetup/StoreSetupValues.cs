using System;
using System.Globalization;
using System.Text.Json;

namespace VirtoCommerce.StoreModule.Core.MCP.StoreSetup;

// Setting and field values as a setup compares them: a value from a request (a JsonElement), a stored one or a default.
public static class StoreSetupValues
{
    // Numbers by value (10 equals 10.0), anything else by its invariant text, ignoring case; null equals an empty text.
    public static bool AreEqual(object left, object right)
    {
        var leftText = ToInvariantString(left);
        var rightText = ToInvariantString(right);

        if (decimal.TryParse(leftText, NumberStyles.Number, CultureInfo.InvariantCulture, out var leftNumber)
            && decimal.TryParse(rightText, NumberStyles.Number, CultureInfo.InvariantCulture, out var rightNumber))
        {
            return leftNumber == rightNumber;
        }

        return string.Equals(leftText ?? string.Empty, rightText ?? string.Empty, StringComparison.OrdinalIgnoreCase);
    }

    public static string ToInvariantString(object value)
    {
        return value switch
        {
            null => null,
            JsonElement { ValueKind: JsonValueKind.String } element => element.GetString(),
            JsonElement { ValueKind: JsonValueKind.Null or JsonValueKind.Undefined } => null,
            JsonElement element => element.GetRawText(),
            DateTime date => date.ToString("O", CultureInfo.InvariantCulture),
            IFormattable formattable => formattable.ToString(null, CultureInfo.InvariantCulture),
            _ => value.ToString(),
        };
    }
}
