using TaskService.Models;
using UserService.Models;

namespace APIGateway.Models;

public class AggregatedResponse
{
    public UserDto User { get; set; } = default(UserDto)!;
    public List<TaskDto> Tasks { get; set; } = [];
}
