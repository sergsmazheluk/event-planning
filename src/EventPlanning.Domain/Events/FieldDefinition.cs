namespace EventPlanning.Domain.Events
{
    public sealed class FieldDefinition
    {
        public string Key { get; init; } = default!;
        public string Label { get; init; } = default!;
        public string Type { get; init; } = "string";
        public bool Required { get; init; }
        public string[]? Options { get; init; }
    }
}
