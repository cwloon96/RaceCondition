using Microsoft.EntityFrameworkCore;
using RaceCondition.Controllers;
using System.Collections.Concurrent;

namespace RaceCondition
{
    public class ReservationQueueProcessor : BackgroundService
    {
        private readonly ILogger _logger;
        private readonly ReservationDbContext _reservationDbContext;

        public static ConcurrentQueue<ReservationRequest> ReservationQueue = new ConcurrentQueue<ReservationRequest>();

        public ReservationQueueProcessor(ILogger<ReservationQueueProcessor> logger)
        {
            _logger = logger;
            _reservationDbContext = new ReservationDbContext();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (true)
            {
                ReservationRequest request;
                if (ReservationQueue.TryDequeue(out request))
                {
                    try
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
                    catch (Exception ex)
                    {
                        _logger.LogInformation(ex.Message);
                    }
                }
                else
                {
                    await Task.Delay(1000); // Wait for a second if no requests are available
                }
            }
        }
    }
}
