namespace BioDomes.Domains.Entities;

public class BiomeEquipment
{
    public Equipment Equipment { get; set; }
    
    public BiomeEquipment(Equipment equipment)
    {
        Equipment = equipment;
    }
}