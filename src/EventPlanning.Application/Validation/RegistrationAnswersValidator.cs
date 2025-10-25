using EventPlanning.Domain.Events;
using System.Text.Json;

namespace EventPlanning.Application.Validation
{
    public interface IRegistrationAnswersValidator
    {
        (bool ok, string[] errors) Validate(JsonElement answers, IReadOnlyCollection<FieldDefinition> fields);
    }

    public sealed class RegistrationAnswersValidator : IRegistrationAnswersValidator
    {
        public (bool ok, string[] errors) Validate(JsonElement answers, 
            IReadOnlyCollection<FieldDefinition> fields)
        {
            var errors = new List<string>();
            var dict = answers.ValueKind == JsonValueKind.Object
                ? answers.EnumerateObject().ToDictionary(p => p.Name, p => p.Value)
                : new Dictionary<string, JsonElement>();

            foreach (var f in fields)
            {
                if (f.Required && !dict.ContainsKey(f.Key))
                    errors.Add($"Missing required field: {f.Key}");
            }

            return (errors.Count == 0, errors.ToArray());
        }
    }
}
