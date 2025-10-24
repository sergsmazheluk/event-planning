using AutoMapper;
using Contracts;
using EventPlanning.Domain.Events;
using EventPlanning.Infrastructure.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Text.Json;

namespace EventPlanning.Api.Controllers
{
    [ApiController]
    [Route("api/events")]
    public sealed class EventsController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly IMapper _mapper;
        public EventsController(AppDbContext db, IMapper mapper) { _db = db; _mapper = mapper; }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> Get(Guid id)
        {
            var ev = await _db.Events.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
            if (ev is null) return NotFound();
            return Ok(_mapper.Map<EventDto>(ev));
        }

        [HttpGet("{id:guid}/schema")]
        public async Task<IActionResult> GetSchema(Guid id)
        {
            var ev = await _db.Events.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
            if (ev is null) return NotFound();
            var def = await _db.EventDefinitions.AsNoTracking().FirstAsync(x => x.Id == ev.EventDefinitionId);
            return Ok(_mapper.Map<EventDefinitionDto>(def));
        }

        [Authorize]
        [HttpPost("{id:guid}/register")]
        public async Task<IActionResult> Register(Guid id, [FromBody] JsonElement answers)
        {
            var ev = await _db.Events.FindAsync(id);
            if (ev is null) return NotFound();

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var exists = await _db.Registrations.AnyAsync(r => r.EventId == id && r.UserId == userId);
            if (exists) return Conflict(new { error = "Already registered" });

            var reg = new Registration(id, userId, answers.GetRawText());
            _db.Registrations.Add(reg);
            ev.IncrementRegistrations();
            await _db.SaveChangesAsync();
            return Ok(new { reg.Id, status = reg.Status });
        }
    }
}
