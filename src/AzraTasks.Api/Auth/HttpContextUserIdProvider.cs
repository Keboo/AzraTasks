using System.Security.Claims;

using AzraTasks.Data.Auth;

namespace AzraTasks.Api.Auth;

public class HttpContextUserIdProvider : IUserIdProvider
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public HttpContextUserIdProvider(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public string UserId
    {
        get
        {
            var user = _httpContextAccessor.HttpContext?.User;
            if (user is null)
            {
                throw new UnauthorizedAccessException("User is not authenticated");
            }

            return GetUserId(user);
        }
    }

    private static string GetUserId(ClaimsPrincipal user)
    {
        return user.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new UnauthorizedAccessException("User is not authenticated.");
    }
}