namespace EventPlanning.Domain.Events
{
    public sealed class EventDefinition
    {
        public Guid Id { get; init; }
        public string Name { get; private set; } = default!;
        public List<FieldDefinition> Fields { get; set; } = new();
        public DateTime CreatedAtUtc { get; init; } = DateTime.UtcNow;

        private EventDefinition() { }

        public EventDefinition(string name) => Name = name;
    }
}
