using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using ModelContextProtocol.AspNetCore.Authentication;

var builder = WebApplication.CreateBuilder(args);

// Load configuration from appsettings
var serverUrl = builder.Configuration["McpServer:ServerUrl"] ?? "http://localhost:5115/";
var tenantId =
    builder.Configuration["McpServer:TenantId"]
    ?? throw new InvalidOperationException("TenantId is required");
var appClientId =
    builder.Configuration["McpServer:AppClientId"]
    ?? throw new InvalidOperationException("AppClientId is required");
var scopeName = builder.Configuration["McpServer:ScopeName"] ?? "mcp.tool";

var authority = $"https://login.microsoftonline.com/{tenantId}/v2.0";

builder
    .Services.AddAuthentication(options =>
    {
        options.DefaultChallengeScheme = McpAuthenticationDefaults.AuthenticationScheme;
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.Authority = authority;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidAudiences = [$"api://{appClientId}"],
            ValidIssuers = [$"https://sts.windows.net/{tenantId}/"],
            NameClaimType = "name",
            RoleClaimType = "roles",
        };
    })
    .AddMcp(options =>
    {
        options.ResourceMetadata = new()
        {
            Resource = new Uri(serverUrl),

            AuthorizationServers = { new Uri(authority) },
            ScopesSupported = [$"api://{appClientId}/{scopeName}"],
        };
    });

builder.Services.AddMcpServer().WithHttpTransport().WithToolsFromAssembly();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
    });
});

builder.Services.AddAuthorization();
builder.Services.AddHttpContextAccessor();

var app = builder.Build();

app.UseCors();
app.UseAuthentication();
app.UseAuthorization();

app.MapMcp("/api/mcp").RequireAuthorization();

app.Run();
