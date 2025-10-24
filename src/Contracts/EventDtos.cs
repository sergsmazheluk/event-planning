namespace Contracts
{
    public record EventDefinitionDto(Guid Id, string Name, List<FieldDefinitionDto> Fields);
    public record EventDto(Guid Id, Guid EventDefinitionId, DateTime StartsAtUtc, 
        int? Capacity, int RegistrationsCount);
}
