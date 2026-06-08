using System.Net;

namespace AzraTasks.Api.Auth;

public static class AuthExtensions
{
    public static void MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var auth = app.MapGroup("/api/auth");

        auth.MapPost("/login", AuthMethods.Login)
            .ProducesProblem((int)HttpStatusCode.Unauthorized);
        auth.MapPost("/register", AuthMethods.Register)
            .ProducesProblem((int)HttpStatusCode.Unauthorized);
        auth.MapPost("/logout", AuthMethods.Logout)
            .RequireAuthorization();
        auth.MapGet("/user", AuthMethods.GetCurrentUser);
    }
}
