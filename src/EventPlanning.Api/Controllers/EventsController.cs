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

        // ✅ Список событий: /api/events
        [HttpGet]
        public async Task<ActionResult<List<EventDto>>> GetAll()
        {
            var events = await _db.Events.AsNoTracking().ToListAsync();
            var dto = _mapper.Map<List<EventDto>>(events);
            return Ok(dto);
        }

        // /api/events/{id}
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> Get(Guid id)
        {
            var ev = await _db.Events.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
            if (ev is null) return NotFound();
            return Ok(_mapper.Map<EventDto>(ev));
        }

        // /api/events/{id}/schema
        [HttpGet("{id:guid}/schema")]
        public async Task<IActionResult> GetSchema(Guid id)
        {
            var ev = await _db.Events.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
            if (ev is null) return NotFound();

            var def = await _db.EventDefinitions.AsNoTracking()
                                               .FirstAsync(x => x.Id == ev.EventDefinitionId);
            return Ok(_mapper.Map<EventDefinitionDto>(def));
        }

        // /api/events/{id}/register
        [Authorize]
        [HttpPost("{id:guid}/register")]
        public async Task<IActionResult> Register(Guid id, [FromBody] JsonElement answers)
        {
            // retry при конфликтах
            const int maxRetries = 3;
            for (var attempt = 1; attempt <= maxRetries; attempt++)
            {
                await using var tx = await _db.Database.BeginTransactionAsync();

                var ev = await _db.Events.Where(e => e.Id == id)
                                         .FirstOrDefaultAsync();

                if (ev is null) return NotFound();

                if (ev.Capacity is int cap && ev.RegistrationsCount >= cap)
                    return Conflict(new { error = "CAPACITY_EXCEEDED" });

                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

                var exists = await _db.Registrations
                                      .AnyAsync(r => r.EventId == id && r.UserId == userId);
                if (exists) return Conflict(new { error = "ALREADY_REGISTERED" });

                _db.Registrations.Add(new Registration(id, userId, answers.GetRawText()));
                ev.IncrementRegistrations();

                try
                {
                    await _db.SaveChangesAsync();
                    await tx.CommitAsync();
                    return Ok(new { status = "registered" });
                }
                catch (DbUpdateConcurrencyException) when (attempt < maxRetries)
                {
                    await tx.RollbackAsync();
                    await Task.Delay(20 * attempt); // экспоненциальная пауза
                    continue; // повторим попытку
                }
            }

            return StatusCode(409, new { error = "CONCURRENCY_CONFLICT" });
        }
    }
}
