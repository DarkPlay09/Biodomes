namespace BioDomes.Domains;

public class Species
{
    public string Name { get; }
    public string Type { get; }
    public string Diet { get; }
    public double AdultSize { get; }

    public Species(string name, string type, string diet, double adultSize)
    {
        Name = name;
        Type = type;
        Diet = diet;
        AdultSize = adultSize;
    }
}