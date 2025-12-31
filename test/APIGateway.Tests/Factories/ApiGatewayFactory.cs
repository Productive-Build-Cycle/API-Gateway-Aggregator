using APIGateway.Tests.ConfigHandlers;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;
using TaskService.Models;
using UserService.Models;

public class ApiGatewayFactory
    : WebApplicationFactory<Runner>
{
    private readonly Func<HttpRequestMessage, HttpResponseMessage>? _handler;

    // 👇 constructor جدید (اختیاری)
    public ApiGatewayFactory(
        Func<HttpRequestMessage, HttpResponseMessage>? handler = null)
    {
        _handler = handler;
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration(config =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["AggregationServices:UsersUrl"] = "https://localhost:5000/users",
                ["AggregationServices:TasksUrl"] = "https://localhost:5002/tasks"
            });
        });

        builder.ConfigureServices(services =>
        {
            services.AddHttpClient("AggregationClient")
                .ConfigurePrimaryHttpMessageHandler(() =>
                    new FakeHttpMessageHandler(
                        _handler ?? DefaultHandler));
        });
    }

    // 👇 رفتار پیش‌فرض (Happy Path)
    private HttpResponseMessage DefaultHandler(HttpRequestMessage request)
    {
        if (request.RequestUri!.ToString().Contains("users"))
        {
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = JsonContent.Create(new[]
                {
                    new UserDto(1, "Saeed", "Nazif"),
                    new UserDto(2, "Arezoo", "Khoshrah"),
                    new UserDto(3, "Soheila", "Golshan"),
                })
            };
        }

        if (request.RequestUri!.ToString().Contains("tasks"))
        {
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = JsonContent.Create(new[]
                {
                    new TaskDto(1, 1, "Task1", UserTaskStatus.ToDo),
                    new TaskDto(2, 2, "Task2", UserTaskStatus.InProgress),
                    new TaskDto(3, 3, "Task3", UserTaskStatus.Done)
                })
            };
        }

        return new HttpResponseMessage(HttpStatusCode.NotFound);
    }
}
