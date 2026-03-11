using BioDomes.Domains.Enums;

namespace BioDomes.Domains.Entities;

public class Biome
{
    public string Name { get; set; }
    public double Temperature { get; set; }
    public double AbsoluteHumidity { get; set; }
    public BiomeState State { get; set; }
    public string? CreatedByUserName { get; set; } // Temporairement en string
    
    public Biome(
        string name,
        double temperature,
        double absoluteHumidity,
        BiomeState state,
        string? createdByUserName) 
        {
        Name = name;
        Temperature = temperature;
        AbsoluteHumidity = absoluteHumidity;
        State = state;
        CreatedByUserName = createdByUserName;
        }
}