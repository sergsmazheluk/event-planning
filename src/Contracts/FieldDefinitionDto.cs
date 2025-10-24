namespace Contracts
{
    public record FieldDefinitionDto(string Key, string Label, string Type, bool Required, string[]? Options);
}
