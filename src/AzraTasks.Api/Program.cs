using AzraTasks.Api.Auth;
using AzraTasks.Core;
using AzraTasks.Data;
using AzraTasks.Api.Middleware;

using Microsoft.AspNetCore.Identity;
using AzraTasks.Data.Auth;
using AzraTasks.Api.Todo;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults()
    .AddDatabase()
    .AddTodo();

builder.Services.AddHttpContextAccessor();
builder.Services.AddSingleton<IUserIdProvider, HttpContextUserIdProvider>();

// Add services to the container.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add CORS for frontend in development
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        if (builder.Environment.IsDevelopment())
        {
            // In development, allow any localhost origin for Vite dev server
            policy.SetIsOriginAllowed(origin => new Uri(origin).Host == "localhost")
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowCredentials();
        }
        else
        {
            var allowedOrigins = builder.Configuration.GetSection("AllowedOrigins").Get<string[]>() ?? [];
            policy.WithOrigins(allowedOrigins)
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowCredentials();
        }
    });
});

// Add authorization policies
builder.Services.AddAuthorization();

var authBuilder = builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = IdentityConstants.ApplicationScheme;
    options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
});

authBuilder.AddIdentityCookies(options =>
{
    options.ApplicationCookie?.Configure(cookieOptions =>
    {
        cookieOptions.Cookie.SecurePolicy = CookieSecurePolicy.Always;

        if (builder.Environment.IsDevelopment())
        {
            // In development, Vite dev server is cross-origin so we need SameSite=None
            cookieOptions.Cookie.SameSite = SameSiteMode.None;
        }
        else
        {
            // In production, frontend and backend are same-site (same eTLD+1),
            // so Lax cookies are sent on cross-origin fetch requests.
            // SameSite=None would be blocked by iOS Safari's ITP.
            cookieOptions.Cookie.SameSite = SameSiteMode.Lax;
        }
    });
});

// No-op email sender for now (can be replaced with real implementation)
builder.Services.AddScoped<IEmailSender<ApplicationUser>>(sp => 
    new NoOpEmailSender<ApplicationUser>());

var app = builder.Build();

app.MapDefaultEndpoints();

// Enable CORS
app.UseCors("AllowFrontend");

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseMigrationsEndPoint();
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
    app.UseHttpsRedirection();
}

// Add exception handling middleware
app.UseMiddleware<ExceptionHandlingMiddleware>();

// Serve static files from React build (production only)
if (!app.Environment.IsDevelopment())
{
    app.UseDefaultFiles();
    app.UseStaticFiles();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapAuthEndpoints();
app.MapTodoListEndpoints();

// SPA fallback for production
if (!app.Environment.IsDevelopment())
{
    app.MapFallbackToFile("index.html");
}

app.Run();

// Simple no-op email sender
internal class NoOpEmailSender<TUser> : IEmailSender<TUser> where TUser : class
{
    public Task SendConfirmationLinkAsync(TUser user, string email, string confirmationLink) => Task.CompletedTask;
    public Task SendPasswordResetLinkAsync(TUser user, string email, string resetLink) => Task.CompletedTask;
    public Task SendPasswordResetCodeAsync(TUser user, string email, string resetCode) => Task.CompletedTask;
}
