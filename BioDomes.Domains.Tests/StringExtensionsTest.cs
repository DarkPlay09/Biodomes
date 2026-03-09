using NUnit.Framework;
using BioDomes.Domains;

public class StringExtensionsTests
{
    [TestCase("Panthera leo", "panthera-leo")]
    [TestCase("Equus quagga", "equus-quagga")]
    [TestCase("Quercus acerifolia", "quercus-acerifolia")]
    public void ToKebabCase_Works(string input, string expected)
    {
        Assert.That(input.ToKebabCase(), Is.EqualTo(expected));
    }
}