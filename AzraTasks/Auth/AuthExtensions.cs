namespace AzraTasks.Auth;

public static class AuthExtensions
{
    public static void MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var auth = app.MapGroup("/api/auth");

        auth.MapPost("/login", AuthMethods.Login);
        auth.MapPost("/register", AuthMethods.Register);
        auth.MapPost("/logout", AuthMethods.Logout)
            .RequireAuthorization();
        auth.MapGet("/user", AuthMethods.GetCurrentUser);
    }
}
