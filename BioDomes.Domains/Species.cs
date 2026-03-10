namespace BioDomes.Domains;

public class Species
{
    public string Name { get; }
    public string Type { get; }
    public string Diet { get; }
    public double AdultSize { get; }
    public string? ImageUrl { get; }

    public Species(string name, string type, string diet, double adultSize, string? imageUrl = null)
    {
        Name = name;
        Type = type;
        Diet = diet;
        AdultSize = adultSize;
        ImageUrl = imageUrl;
    }
}