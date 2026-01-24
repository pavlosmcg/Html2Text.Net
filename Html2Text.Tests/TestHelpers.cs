using NUnit.Framework;

namespace Html2Text.Tests;

internal class TestHelpers
{
    public static void AssertAreEqualNormalised(string result, string expected)
    {
        Assert.That(Normalise(result), Is.EqualTo(Normalise(expected)));
    }

    private static string Normalise(string s) =>
        s.Replace("\r\n", "\n");
}
