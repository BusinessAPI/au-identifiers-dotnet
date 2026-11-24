using System.Linq;

namespace BusinessApi.AuIdentifiers;

public static class Abn
{
    private static readonly int[] Weights = { 10, 1, 3, 5, 7, 9, 11, 13, 15, 17, 19 };

    public static bool IsValid(string? value) => Normalise(value) != null;

    public static string? Normalise(string? value)
    {
        var abn = Digits.Of(value, 11);
        if (abn == null) return null;
        // Subtract 1 from the first digit, then the weighted sum must be divisible by 89.
        var sum = (abn[0] - '0' - 1) * Weights[0] + Digits.Sum(abn.Substring(1), Weights.Skip(1).ToArray());
        return sum % 89 == 0 ? abn : null;
    }

    public static string? Format(string? value)
    {
        var abn = Normalise(value);
        return abn == null ? null : $"{abn.Substring(0, 2)} {abn.Substring(2, 3)} {abn.Substring(5, 3)} {abn.Substring(8)}";
    }
}

public static class Acn
{
    private static readonly int[] Weights = { 8, 7, 6, 5, 4, 3, 2, 1 };

    public static bool IsValid(string? value) => Normalise(value) != null;

    public static string? Normalise(string? value)
    {
        var acn = Digits.Of(value, 9);
        if (acn == null) return null;
        return acn[8] - '0' == (10 - Digits.Sum(acn, Weights) % 10) % 10 ? acn : null;
    }

    public static string? Format(string? value)
    {
        var acn = Normalise(value);
        return acn == null ? null : $"{acn.Substring(0, 3)} {acn.Substring(3, 3)} {acn.Substring(6)}";
    }
}

public static class Arbn
{
    public static bool IsValid(string? value) => Acn.IsValid(value);
    public static string? Normalise(string? value) => Acn.Normalise(value);
    public static string? Format(string? value) => Acn.Format(value);
}

public static class Tfn
{
    private static readonly int[] Weights9 = { 1, 4, 3, 7, 5, 8, 6, 9, 10 };
    private static readonly int[] Weights8 = { 1, 4, 3, 7, 5, 8, 6, 10 };

    public static bool IsValid(string? value, bool individual = false) => Normalise(value, individual) != null;

    public static string? Normalise(string? value, bool individual = false)
    {
        // 8-digit TFNs (organisations only) have their own weights. A 9-digit TFN with a leading
        // zero passes on either the 9-digit check or the 8-digit check of the rest.
        var tfn = individual ? Digits.Of(value, 9) : Digits.Of(value, 8, 9);
        if (tfn == null) return null;
        if (tfn.Length == 9 && tfn.TrimStart('0').Length >= 8 && Digits.Sum(tfn, Weights9) % 11 == 0) return tfn;
        var core = tfn.TrimStart('0');
        var weights = core.Length == 9 ? Weights9 : core.Length == 8 ? Weights8 : null;
        return weights != null && Digits.Sum(core, weights) % 11 == 0 ? tfn : null;
    }
}

public static class DirectorId
{
    public static bool IsValid(string? value) => Normalise(value) != null;

    // Format only. The check digit method is only given to ATO-registered software providers.
    public static string? Normalise(string? value)
    {
        var id = Digits.Of(value, 15);
        return id != null && id.StartsWith("036", System.StringComparison.Ordinal) ? id : null;
    }
}

public static class Vin
{
    private const string Allowed = "ABCDEFGHJKLMNPRSTUVWXYZ0123456789";

    public static bool IsValid(string? value) => Normalise(value) != null;

    public static string? Normalise(string? value)
    {
        if (value == null) return null;
        var trimmed = value.Trim(Digits.Whitespace);
        if (trimmed.Any(c => c > 0x7f)) return null;
        var vin = trimmed.ToUpperInvariant();
        return vin.Length == 17 && vin.All(c => Allowed.IndexOf(c) >= 0) ? vin : null;
    }
}

internal static class Digits
{
    public static readonly char[] Whitespace = { ' ', '\t', '\n', '\r', '\v', '\f' };

    public static string? Of(string? value, params int[] lengths)
    {
        if (value == null) return null;
        var compact = new string(value.Where(c => c != '-' && System.Array.IndexOf(Whitespace, c) < 0).ToArray());
        return lengths.Contains(compact.Length) && compact.All(c => c >= '0' && c <= '9') ? compact : null;
    }

    public static int Sum(string digits, int[] weights)
    {
        var sum = 0;
        for (var i = 0; i < weights.Length; i++) sum += (digits[i] - '0') * weights[i];
        return sum;
    }
}
