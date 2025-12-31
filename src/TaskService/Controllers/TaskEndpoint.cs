using TaskService.Abstractions;
using TaskService.Models;

namespace TaskService.Controllers;

public class TaskEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/tasks", () =>
             new List<TaskDto>
             {
                new(1, 1, "Task1", UserTaskStatus.ToDo),
                new(2, 2, "Task2", UserTaskStatus.InProgress),
                new(3,3, "Task1", UserTaskStatus.Done),
             });
    }
}
