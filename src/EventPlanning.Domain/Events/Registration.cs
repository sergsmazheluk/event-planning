namespace EventPlanning.Domain.Events
{
    public sealed class Registration
    {
        public Guid Id { get; init; } = Guid.NewGuid();
        public Guid EventId { get; init; }
        public string UserId { get; init; } = default!;
        public string AnswersJson { get; init; } = "{}";
        public string Status { get; private set; } = "Pending";

        private Registration() { }

        public Registration(Guid eventId, string userId, string answers)
        {
            EventId = eventId;
            UserId = userId;
            AnswersJson = answers;
        }

        public void Confirm() => Status = "Confirmed";
    }
}
