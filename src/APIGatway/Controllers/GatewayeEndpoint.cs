using APIGateway.Abstractions;
using APIGateway.Models;
using TaskService.Models;
using UserService.Models;

namespace APIGateway.Controllers;

public class GatewayeEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/aggregate", async (
            HttpContext context,
            IConfiguration config,
            IHttpClientFactory clientFactory) =>
        {
            try
            {
                var usersUrl = config["AggregationServices:UsersUrl"];
                var tasksUrl = config["AggregationServices:TasksUrl"];

                var client = clientFactory.CreateClient("AggregationClient");

                var usersTask = client.GetAsync(usersUrl);
                var tasksTask = client.GetAsync(tasksUrl);

                await Task.WhenAll(usersTask, tasksTask);

                var usersResponse = await usersTask;
                var tasksResponse = await tasksTask;

                if (!usersResponse.IsSuccessStatusCode || !tasksResponse.IsSuccessStatusCode)
                {
                    return Results.Problem("Error fetching data from services.");
                }

                var users = await usersResponse.Content.ReadFromJsonAsync<List<UserDto>>();
                var tasks = await tasksResponse.Content.ReadFromJsonAsync<List<TaskDto>>();

                var aggregateds = new List<AggregatedResponse>();
                foreach (var user in users!.ToList())
                {
                    var aggregate = new AggregatedResponse
                    {
                        User = user,
                        Tasks = tasks!.Where(_ => _.UserId == user.Id)
                        .Select(t =>
                        new TaskDto(t.Id, user.Id, t.Name, t.Status))
                        .ToList()
                    };
                    aggregateds.Add(aggregate);
                }

                return Results.Ok(aggregateds);
            }
            catch (Exception ex)
            {
                return Results.Problem(ex.Message);
            }
        })
        .WithName("AggregateData");
    }
}
