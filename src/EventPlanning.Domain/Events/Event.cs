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

        public Event(Guid defId, DateTime startsAt, int? capacity, string? custom)
        {
            EventDefinitionId = defId;
            StartsAtUtc = startsAt;
            Capacity = capacity;
            CustomDataJson = custom;
        }

        public void IncrementRegistrations() => RegistrationsCount++;
    }
}
