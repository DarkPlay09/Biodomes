using BioDomes.Domains.Extensions;
using NUnit.Framework;

namespace BioDomes.Domains.Tests.Extensions;

[TestFixture]
public class StringExtensionsTests
{
    [TestCase(null, "")]
    [TestCase("", "")]
    [TestCase("   ", "")]
    public void ShouldReturnEmptyString_WhenInputIsNullEmptyOrWhiteSpace(string? input, string expected)
    {
        var result = input.ToKebabCase();

        Assert.That(result, Is.EqualTo(expected));
    }

    [TestCase("Hello World", "hello-world")]
    [TestCase("HELLO WORLD", "hello-world")]
    [TestCase("  Hello World  ", "hello-world")]
    public void ShouldReturnLowerKebabCase_WhenInputContainsUppercaseOrSpaces(string input, string expected)
    {
        var result = input.ToKebabCase();

        Assert.That(result, Is.EqualTo(expected));
    }

    [TestCase("Érable à sucre", "erable-a-sucre")]
    [TestCase("Déjà Vu", "deja-vu")]
    [TestCase("Forêt tropicale humide", "foret-tropicale-humide")]
    [TestCase("Éléphant d’Afrique", "elephant-d-afrique")]
    public void ShouldRemoveAccents_WhenInputContainsAccentedCharacters(string input, string expected)
    {
        var result = input.ToKebabCase();

        Assert.That(result, Is.EqualTo(expected));
    }

    [TestCase("Hello___World!!!", "hello-world")]
    [TestCase("Hello---World", "hello-world")]
    [TestCase("Hello / World", "hello-world")]
    [TestCase("Hello, World!", "hello-world")]
    [TestCase("Hello@World#2026", "hello-world-2026")]
    public void ShouldReplaceSpecialCharactersWithSingleDash_WhenInputContainsSeparators(string input, string expected)
    {
        var result = input.ToKebabCase();

        Assert.That(result, Is.EqualTo(expected));
    }

    [TestCase("---Hello World---", "hello-world")]
    [TestCase("___Hello World___", "hello-world")]
    [TestCase("!!!Hello World!!!", "hello-world")]
    public void ShouldTrimDashes_WhenInputStartsOrEndsWithSpecialCharacters(string input, string expected)
    {
        var result = input.ToKebabCase();

        Assert.That(result, Is.EqualTo(expected));
    }

    [TestCase("Biome 1", "biome-1")]
    [TestCase("Biodome 2026 Version 2", "biodome-2026-version-2")]
    [TestCase("Zone 51", "zone-51")]
    public void ShouldKeepNumbers_WhenInputContainsDigits(string input, string expected)
    {
        var result = input.ToKebabCase();

        Assert.That(result, Is.EqualTo(expected));
    }
}