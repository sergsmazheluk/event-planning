using EventPlanning.Domain.Events;
using FluentAssertions;
using System.Text.Json;

public class EventDefinitionJsonTests
{
    [Fact]
    public void Fields_Serialized_And_Deserialized()
    {
        var def = new EventDefinition("Test")
        {
            Fields = new() { new FieldDefinition { Key = "topic", Label = "Тема", Required = true } }
        };
        var json = JsonSerializer.Serialize(def.Fields);
        var back = JsonSerializer.Deserialize<List<FieldDefinition>>(json)!;
        back.Should().HaveCount(1);
        back[0].Key.Should().Be("topic");
        back[0].Required.Should().BeTrue();
    }
}
