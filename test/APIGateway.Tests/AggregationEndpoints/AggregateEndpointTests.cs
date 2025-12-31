using APIGateway.Models;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using System.Net;
using System.Net.Http.Json;
using TaskService.Models;
using UserService.Models;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;

public class GatewayTests : IDisposable
{
    private readonly WireMockServer _userServer;
    private readonly WireMockServer _taskServer;
    private readonly HttpClient _gatewayClient;

    public GatewayTests()
    {
        _userServer = WireMockServer.Start();
        _taskServer = WireMockServer.Start();

        var factory = new WebApplicationFactory<Runner>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureAppConfiguration((context, config) =>
                {
                    config.AddInMemoryCollection(new Dictionary<string, string?>
                    {
                        ["AggregationServices:UsersUrl"] = $"{_userServer.Url}/users",
                        ["AggregationServices:TasksUrl"] = $"{_taskServer.Url}/tasks"
                    });
                });
            });

        _gatewayClient = factory.CreateClient();
    }

    [Fact]
    public async Task Aggregate_WhenAllServicesAreUp_ReturnsAggregatedData()
    {
        var mockUsers = new List<UserDto> { new(1, "Ali", "Alavi") };
        var mockTasks = new List<TaskDto> { new(101, 1, "Task 1", 0) };

        _userServer.Given(Request.Create().WithPath("/users").UsingGet())
            .RespondWith(Response.Create().WithStatusCode(200).WithBodyAsJson(mockUsers));

        _taskServer.Given(Request.Create().WithPath("/tasks").UsingGet())
            .RespondWith(Response.Create().WithStatusCode(200).WithBodyAsJson(mockTasks));

        var response = await _gatewayClient.GetAsync("/aggregate");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<List<AggregatedResponse>>();
        result.Should().NotBeNull();
        result![0].User.FirstName.Should().Be("Ali");
        result[0].Tasks.Should().HaveCount(1);
    }

    [Fact]
    public async Task Aggregate_WhenUserServiceIsDown_ReturnsProblem()
    {
        _userServer.Given(Request.Create().WithPath("/users").UsingGet())
            .RespondWith(Response.Create().WithStatusCode(500));

        _taskServer.Given(Request.Create().WithPath("/tasks").UsingGet())
            .RespondWith(Response.Create().WithStatusCode(200).WithBodyAsJson(new List<TaskDto>()));

        var response = await _gatewayClient.GetAsync("/aggregate");

        response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
    }

    [Fact]
    public async Task Aggregate_WhenTaskServiceIsDown_ReturnsProblem()
    {
        _userServer.Given(Request.Create().WithPath("/users").UsingGet())
            .RespondWith(Response.Create().WithStatusCode(200).WithBodyAsJson(new List<UserDto> { new(1, "Ali", "Alavi") }));

        _taskServer.Given(Request.Create().WithPath("/tasks").UsingGet())
            .RespondWith(Response.Create().WithStatusCode(404));

        var response = await _gatewayClient.GetAsync("/aggregate");

        response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
    }

    public void Dispose()
    {
        _userServer.Stop();
        _taskServer.Stop();
        _userServer.Dispose();
        _taskServer.Dispose();
    }
}