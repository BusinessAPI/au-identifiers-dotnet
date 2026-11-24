using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Xunit;

namespace BusinessApi.AuIdentifiers.Tests;

public class VectorsTests
{
    private static readonly JsonElement Vectors =
        JsonDocument.Parse(File.ReadAllText(Path.Combine(System.AppContext.BaseDirectory, "vectors.json"))).RootElement;

    public static IEnumerable<object?[]> Cases(string key)
    {
        foreach (var c in Vectors.GetProperty(key).EnumerateArray())
        {
            var individual = c.TryGetProperty("individual", out var i) && i.GetBoolean();
            var expected = key == "valid" ? (object)c.GetProperty("valid").GetBoolean()
                : c.GetProperty("expected").ValueKind == JsonValueKind.Null ? null : c.GetProperty("expected").GetString();
            yield return new[] { c.GetProperty("type").GetString(), c.GetProperty("value").GetString(), individual, expected };
        }
    }

    [Theory]
    [MemberData(nameof(Cases), "valid")]
    public void Valid(string type, string value, bool individual, bool expected)
    {
        var got = type switch
        {
            "abn" => Abn.IsValid(value),
            "acn" => Acn.IsValid(value),
            "tfn" => Tfn.IsValid(value, individual),
            "did" => DirectorId.IsValid(value),
            "vin" => Vin.IsValid(value),
            _ => throw new System.ArgumentException(type),
        };
        Assert.Equal(expected, got);
    }

    [Theory]
    [MemberData(nameof(Cases), "format")]
    public void Format(string type, string value, bool _, string? expected)
    {
        var got = type switch
        {
            "abn" => Abn.Format(value),
            "acn" => Acn.Format(value),
            "vin" => Vin.Normalise(value),
            _ => throw new System.ArgumentException(type),
        };
        Assert.Equal(expected, got);
    }

    [Fact]
    public void OtherInputs()
    {
        Assert.True(Arbn.IsValid("000 000 019"));
        Assert.False(Arbn.IsValid("000 000 018"));
        Assert.False(Abn.IsValid(null));
        Assert.False(Tfn.IsValid(null));
        Assert.False(Acn.IsValid("٠٠٤٠٨٥٦١٦"));
    }
}
