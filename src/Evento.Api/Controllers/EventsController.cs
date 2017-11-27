using System;
using System.Threading.Tasks;
using Evento.Infrastructure.Commands.Events;
using Evento.Infrastructure.Services;
using Microsoft.AspNetCore.Mvc;

namespace Evento.Api.Controllers
{
    [Route("events")]
    public class EventsController : ApiControllerBase
    {
        private readonly IEventService _eventService;
        public EventsController(IEventService eventService)
        {
            _eventService = eventService;
        }
        
        [HttpGet]
        public async Task<IActionResult> Get(string name)
        {
            var events = await _eventService.BrowseAsync(name);
            return Json(events);
        }

        [HttpGet("{eventId}")]
        public async Task<IActionResult> Get(Guid eventId)
        {
            var @event = await _eventService.GetAsync(eventId);
            if(@event == null)
            {
                return NotFound();
            }

            return Json(@event);
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody]CreateEvent command)
        {
            command.Id = Guid.NewGuid();
            await _eventService.CreateAsync(command.Id, command.Name,
            command.Description, command.StartDate, command.EndDate);
            await _eventService.AddTicketsAsync(command.Id, command.Tickets, command.Price);
            return Created($"/events/{command.Id}", null); // 201 - url z nowo utworzonym servisem 
        }

        [HttpPut("{eventId}")]
        public async Task<IActionResult> Put(Guid eventId, [FromBody] UpdateEvent command)
        {
            await _eventService.UpdateAsync(eventId, command.Name, command.Description);
            return NoContent(); //204 - zraca ze wszytsko ok 
        }

        [HttpDelete("{eventId}")]
        public async Task<IActionResult> Delete(Guid eventId)
        {
            await _eventService.DeleteAsync(eventId);
            return NoContent(); //204 - zraca ze wszytsko ok 
        }
    }
}