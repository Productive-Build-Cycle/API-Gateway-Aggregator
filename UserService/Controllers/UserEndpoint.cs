using UserService.Abstractions;
using UserService.Models;

namespace UserService.Controllers;

public class UserEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/users", () =>
             new List<UserDto>
             {
                 new(1, "Soheila", "Golshan"),
                 new(2, "Arezoo", "Khoshrah"),
                 new(3, "Saeed", "Nazif"),
             });
    }
}
