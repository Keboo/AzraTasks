using System.Security.Claims;

using AzraTasks.Data;

using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;

namespace AzraTasks.Auth;

public static class AuthMethods
{
    public static async Task<IResult> Login(
        LoginRequest request,
        SignInManager<ApplicationUser> signInManager,
        UserManager<ApplicationUser> userManager)
    {
        var user = await userManager.FindByEmailAsync(request.Email);
        if (user is null)
        {
            return TypedResults.Json(
                new { message = "Invalid email or password" },
                statusCode: StatusCodes.Status401Unauthorized);
        }

        var result = await signInManager.PasswordSignInAsync(
            user,
            request.Password,
            request.RememberMe ?? false,
            lockoutOnFailure: false);

        if (result.Succeeded)
        {
            return TypedResults.Ok(new UserInfo
            {
                UserId = user.Id,
                UserName = user.UserName ?? "",
                Email = user.Email ?? "",
                IsAuthenticated = true
            });
        }

        return TypedResults.Json(
            new { message = "Invalid email or password" },
            statusCode: StatusCodes.Status401Unauthorized);
    }

    public static async Task<IResult> Register(
        RegisterRequest request,
        SignInManager<ApplicationUser> signInManager,
        UserManager<ApplicationUser> userManager)
    {
        var user = new ApplicationUser
        {
            UserName = request.Email,
            Email = request.Email
        };
        var result = await userManager.CreateAsync(user, request.Password);

        if (result.Succeeded)
        {
            // Automatically verify the email.
            // TODO: Implement email verification.
            var token = await userManager.GenerateEmailConfirmationTokenAsync(user);
            result = await userManager.ConfirmEmailAsync(user, token);

            if (result.Succeeded)
            {
                await signInManager.SignInAsync(user, isPersistent: false);
                return TypedResults.Ok(new UserInfo
                {
                    UserId = user.Id,
                    UserName = user.UserName ?? "",
                    Email = user.Email ?? "",
                    IsAuthenticated = true
                });
            }
        }

        return TypedResults.BadRequest(
            new { errors = result.Errors.Select(e => e.Description) });
    }

    public static async Task<NoContent> Logout(SignInManager<ApplicationUser> signInManager)
    {
        await signInManager.SignOutAsync();
        return TypedResults.NoContent();
    }

    public static Ok<UserInfo> GetCurrentUser(
        ClaimsPrincipal user,
        UserManager<ApplicationUser> userManager)
    {
        if (user.Identity?.IsAuthenticated != true)
        {
            return TypedResults.Ok(new UserInfo { IsAuthenticated = false });
        }

        return TypedResults.Ok(new UserInfo
        {
            UserId = userManager.GetUserId(user) ?? "",
            UserName = user.Identity?.Name ?? "",
            Email = user.Identity?.Name ?? "",
            IsAuthenticated = true
        });
    }
}

public sealed record LoginRequest(string Email, string Password, bool? RememberMe);

public sealed record RegisterRequest(string Email, string Password, string ConfirmPassword);

public sealed record UserInfo
{
    public string UserId { get; init; } = "";
    public string UserName { get; init; } = "";
    public string Email { get; init; } = "";
    public bool IsAuthenticated { get; init; }
}
