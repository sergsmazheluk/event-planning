using AutoMapper;
using EventPlanning.Application.Mappings;       
using EventPlanning.Domain.Events;              
using Contracts;           
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions; 

namespace EventPlanning.UnitTests;

public class AutoMapperTests
{
    private static IMapper CreateMapper()
    {
        // Точно так же, как мы регистрируем в Program.cs — стабильный способ для разных версий AutoMapper.
        var expr = new MapperConfigurationExpression();
        expr.AddProfile<EventProfile>();

        var config = new MapperConfiguration(expr, NullLoggerFactory.Instance);
        // Если конфигурация неверна — упадёт здесь или в AssertConfigurationIsValid()
        config.AssertConfigurationIsValid();
        return new Mapper(config);
    }

    [Fact]
    public void Configuration_Should_Be_Valid()
    {
        // Просто валидируем профили
        var expr = new MapperConfigurationExpression();
        expr.AddProfile<EventProfile>();
        var config = new MapperConfiguration(expr, NullLoggerFactory.Instance);

        // не выбрасывает исключений
        config.AssertConfigurationIsValid();
    }

    [Fact]
    public void Map_FieldDefinition_To_Dto()
    {
        var mapper = CreateMapper();

        var field = new FieldDefinition
        {
            Key = "topic",
            Label = "Тема",
            Type = "string",
            Required = true,
            Options = new[] { "Sport", "Music" }
        };

        var dto = mapper.Map<FieldDefinitionDto>(field);

        dto.Key.Should().Be("topic");
        dto.Label.Should().Be("Тема");
        dto.Type.Should().Be("string");
        dto.Required.Should().BeTrue();
        dto.Options.Should().Contain(new[] { "Sport", "Music" });
    }

    [Fact]
    public void Map_EventDefinition_To_Dto_With_Fields()
    {
        var mapper = CreateMapper();

        var def = new EventDefinition("Тест")
        {
            Id = Guid.NewGuid(),
            Fields =
            {
                new FieldDefinition { Key="topic", Label="Тема", Type="string", Required=true },
                new FieldDefinition { Key="city",  Label="Город", Type="string", Required=false }
            }
        };

        var dto = mapper.Map<EventDefinitionDto>(def);

        dto.Id.Should().Be(def.Id);
        dto.Name.Should().Be("Тест");
        dto.Fields.Should().HaveCount(2);
        dto.Fields.Select(f => f.Key).Should().Contain(new[] { "topic", "city" });
        dto.Fields.First(f => f.Key == "topic").Required.Should().BeTrue();
    }

    [Fact]
    public void Map_Event_To_Dto()
    {
        var mapper = CreateMapper();

        var defId = Guid.NewGuid();
        var starts = DateTime.UtcNow.AddDays(1);
        var ev = new Event(defId, starts, capacity: 100, customDataJson: "{\"place\":\"Hall A\"}");

        var dto = mapper.Map<EventDto>(ev);

        dto.Id.Should().Be(ev.Id);
        dto.EventDefinitionId.Should().Be(defId);
        dto.StartsAtUtc.Should().BeCloseTo(starts, TimeSpan.FromSeconds(1));
        dto.Capacity.Should().Be(100);
        dto.RegistrationsCount.Should().Be(0);
    }
}


