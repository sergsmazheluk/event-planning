namespace EventPlanning.Domain.Events
{
    public sealed class Event
    {
        public Guid Id { get; init; } = Guid.NewGuid();
        public Guid EventDefinitionId { get; init; }
        public DateTime StartsAtUtc { get; private set; }
        public int? Capacity { get; private set; }
        public string? CustomDataJson { get; private set; }
        public int RegistrationsCount { get; private set; }

        private Event() { }

        public Event(Guid eventDefinitionId, DateTime startsAtUtc, int? capacity, string? customDataJson)
        {
            EventDefinitionId = eventDefinitionId;
            StartsAtUtc = startsAtUtc;
            Capacity = capacity;
            CustomDataJson = customDataJson;
        }

        public void IncrementRegistrations() => RegistrationsCount++;
    }
}
