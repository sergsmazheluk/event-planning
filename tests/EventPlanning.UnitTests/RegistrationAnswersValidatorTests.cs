using System.Text.Json;
using EventPlanning.Application.Validation;
using EventPlanning.Domain.Events;
using FluentAssertions;

namespace EventPlanning.UnitTests
{
    public class RegistrationAnswersValidatorTests
    {
        private readonly IRegistrationAnswersValidator _v = new RegistrationAnswersValidator();

        [Fact]
        public void Should_Fail_When_Required_Missing()
        {
            var fields = new List<FieldDefinition>
        {
            new() { Key = "topic", Label = "Тема", Required = true, Type = "string" },
            new() { Key = "city",  Label = "Город", Required = false, Type = "string" }
        };
            var answers = JsonDocument.Parse("""{ "city": "Minsk" }""").RootElement;

            var (ok, errs) = _v.Validate(answers, fields);

            ok.Should().BeFalse();
            errs.Should().Contain(e => e.Contains("topic"));
        }

        [Fact]
        public void Should_Pass_When_All_Required_Present()
        {
            var fields = new List<FieldDefinition>
        {
            new() { Key = "topic", Label = "Тема", Required = true, Type = "string" },
            new() { Key = "city",  Label = "Город", Required = false, Type = "string" }
        };
            var answers = JsonDocument.Parse("""{ "topic": "Music", "city": "Minsk" }""").RootElement;

            var (ok, errs) = _v.Validate(answers, fields);

            ok.Should().BeTrue();
            errs.Should().BeEmpty();
        }
    }
}
