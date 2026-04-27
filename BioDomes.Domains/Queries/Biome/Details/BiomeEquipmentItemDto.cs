namespace BioDomes.Domains.Queries;

public sealed class BiomeEquipmentItemDto
{
    public int EquipmentId { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? ImagePath { get; init; }
    public string? ProducedElement { get; init; }
    public string? ConsumedElement { get; init; }
}