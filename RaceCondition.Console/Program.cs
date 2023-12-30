// See https://aka.ms/new-console-template for more information
using System.Net.Http.Json;

Console.WriteLine("Start!");

const int numUsers = 10;  // Adjust as needed
const string apiUrl = "http://localhost:5196/Reservation/";
const string ReserveTicketUrl = "ReserveTicket";
const string ReserveTicketWithSemaphoreUrl = "ReserveTicketWithSemaphore";
const string ReserveTicketWithVersionUrl = "ReserveTicketWithVersion";
const string ReserveTicketWithQueueUrl = "ReserveTicketWithQueue";

Thread.Sleep(3000);

var tasks = new Task[numUsers];
for (int i = 0; i < numUsers; i++)
{
    tasks[i] = SimulateUserAsync(apiUrl + ReserveTicketWithQueueUrl, "user_" + i);
}

await Task.WhenAll(tasks);
Console.WriteLine("Completed");

static async Task SimulateUserAsync(string apiUrl, string name)
{
    using (var client = new HttpClient())
    {
        try
        {
            var request = new HttpRequestMessage(HttpMethod.Post, apiUrl);  // Adjust HTTP method as needed
            request.Content = JsonContent.Create(new { ticketId = 1,  name });

            var response = await client.SendAsync(request);
            response.EnsureSuccessStatusCode();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"User request failed: {ex.Message}");
        }
    }
}