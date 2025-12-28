namespace TaskService.Models;

public record TaskDto(
    int Id,
    int UserId,
    string Name,
    UserTaskStatus Status);
