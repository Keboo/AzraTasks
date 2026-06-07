using AzraTasks.Data.Auth;

namespace AzraTasks.Core.Auth;

public class NullUserIdProvider : IUserIdProvider
{
    public string UserId { get; } = "";
}
