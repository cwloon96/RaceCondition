using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Concurrent;

namespace RaceCondition.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ReservationController : ControllerBase
    {
        private readonly ILogger<ReservationController> _logger;
        private readonly ReservationDbContext _reservationDbContext;
        private static Semaphore _semaphore = new Semaphore(1, 1);

        public ReservationController(ILogger<ReservationController> logger)
        {
            _logger = logger;
            _reservationDbContext = new ReservationDbContext();
        }

        [HttpPost("ReserveTicket")]
        public async Task<IActionResult> ReserveTicket(ReservationRequest request)
        {
            try
            {
                await ReserveTicketAsync(request);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogInformation(ex.Message);
                return BadRequest();
            }
        }

        [HttpPost("ReserveTicketWithSemaphore")]
        public async Task<IActionResult> ReserveTicketWithSemaphore(ReservationRequest request)
        {
            try
            {
                if (_semaphore.WaitOne(5000))
                {
                    try
                    {
                        await ReserveTicketAsync(request);
                        return Ok();
                    }
                    catch (Exception ex)
                    {
                        _logger.LogInformation(ex.Message);
                        return BadRequest();
                    }
                }

                _logger.LogInformation("Couldn't receive signal");
                return BadRequest();
            }
            finally
            {
                _semaphore.Release();
            }
        }

        [HttpPost("ReserveTicketWithVersion")]
        public async Task<IActionResult> ReserveTicketWithVersion(ReservationRequest request)
        {
            var ticket = await _reservationDbContext.TicketWithVersion.FirstOrDefaultAsync(t => t.Id == request.TicketId);

            if (!ticket.Available)
            {
                _logger.LogInformation("Ticket is not available");
                return BadRequest();
            }

            ticket.Available = false;
            ticket.Name = request.Name;
            _reservationDbContext.Update(ticket);
            try
            {
                await _reservationDbContext.SaveChangesAsync();
                _logger.LogInformation("Ticket is booked by {0}", request.Name);
            } 
            catch (Exception ex)
            {
                _logger.LogInformation("Failed to SaveChangesAsync", ex.ToString());
            }


            return Ok();
        }

        [HttpPost("ReserveTicketWithQueue")]
        public async Task<IActionResult> ReserveTicketWithQueue(ReservationRequest request)
        {
            ReservationQueueProcessor.ReservationQueue.Enqueue(request);

            return Ok();
        }

        private async Task ReserveTicketAsync(ReservationRequest request)
        {
            var ticket = await _reservationDbContext.Tickets.FirstOrDefaultAsync(t => t.Id == request.TicketId);

            if (!ticket.Available)
            {
                throw new Exception("Ticket is not available");
            }

            ticket.Available = false;
            ticket.Name = request.Name;
            _reservationDbContext.Update(ticket);
            await _reservationDbContext.SaveChangesAsync();

            _logger.LogInformation("Ticket is booked by {0}", request.Name);
        }
    }
}

public class ReservationRequest
{
    public int TicketId { get; set; }

    public string Name { get; set; }
}
