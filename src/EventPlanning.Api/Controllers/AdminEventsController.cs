using AutoMapper;
using Contracts;
using EventPlanning.Domain.Events;
using EventPlanning.Infrastructure.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace EventPlanning.Api.Controllers
{
    [ApiController]
    [Route("api/admin/events")]
    public sealed class AdminEventsController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly IMapper _mapper;
        public AdminEventsController(AppDbContext db, IMapper mapper) { _db = db; _mapper = mapper; }

        public record CreateDefinitionDto(string Name, List<FieldDefinition> Fields);

        [HttpPost("definitions")]
        public async Task<IActionResult> CreateDefinition([FromBody] CreateDefinitionDto dto)
        {
            var def = new EventDefinition(dto.Name) { Id = Guid.NewGuid(), Fields = dto.Fields };
            _db.EventDefinitions.Add(def);
            await _db.SaveChangesAsync();
            return Ok(_mapper.Map<EventDefinitionDto>(def));
        }

        public record CreateEventDto(Guid EventDefinitionId, DateTime StartsAtUtc, int? Capacity, 
            object? CustomData);

        [HttpPost]
        public async Task<IActionResult> CreateEvent([FromBody] CreateEventDto dto)
        {
            var e = new Event(dto.EventDefinitionId, dto.StartsAtUtc, dto.Capacity,
                dto.CustomData is null ? null : JsonSerializer.Serialize(dto.CustomData));
            _db.Events.Add(e);
            await _db.SaveChangesAsync();
            return Ok(_mapper.Map<EventDto>(e));
        }
    }
}
