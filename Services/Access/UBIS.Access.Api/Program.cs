using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using UBIS.Access.Application.Interfaces;
using UBIS.Access.Infrastructure.Services;
using UBIS.Access.Api.Services;
using UBIS.Access.Infrastructure.Data;
using UBIS.Access.Infrastructure.Repositories.Interfaces;
using UBIS.Access.Infrastructure.Repositories.Implementations;
using UBIS.Access.Api.Middleware;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<AccessDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("AccessDatabase")));

// ✅ Repositories
builder.Services.AddScoped<IRoleRepos, RoleRepos>();
builder.Services.AddScoped<IUserRepos, UserRepos>();
builder.Services.AddScoped<IPermissionRepos, PermissionRepos>();
builder.Services.AddScoped<IRolePermissionRepos, RolePermissionRepos>();
builder.Services.AddScoped<IUserRoleRepos, UserRoleRepos>();

// ✅ Services 
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IEntraGraphService, EntraGraphService>();
builder.Services.AddScoped<IPermissionService, PermissionService>();
builder.Services.AddScoped<IRoleService, RoleService>();
builder.Services.AddScoped<IRolePermissionService, RolePermissionService>();
builder.Services.AddScoped<IUserRoleService, UserRoleService>();

var jwtSecret = builder.Configuration["Jwt:Secret"]
    ?? throw new InvalidOperationException("Jwt:Secret ยังไม่ได้ตั้งค่า");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                System.Text.Encoding.UTF8.GetBytes(jwtSecret))
        };
    })
    .AddJwtBearer("EntraID", options =>
    {
        options.Authority = $"https://login.microsoftonline.com/{builder.Configuration["Azure:TenantId"]}/v2.0";
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidAudience = builder.Configuration["Azure:ClientId"]
        };
        options.Events = new JwtBearerEvents
        {
            OnAuthenticationFailed = context => Task.CompletedTask,
            OnTokenValidated = context => Task.CompletedTask
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("system.admin", policy => policy.RequireAssertion(ctx =>
        ctx.User.HasClaim(c => c.Type == "perm" &&
            (c.Value == "*" || c.Value.StartsWith("*:") ||
             c.Value == "system.admin" || c.Value.StartsWith("system.admin:")))));
});

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins(
                  "http://localhost:5173",
                  "http://127.0.0.1:5173")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    try
    {
        var db = scope.ServiceProvider.GetRequiredService<AccessDbContext>();
        _ = db.Model;
        await db.Database.ExecuteSqlRawAsync("SELECT 1");
    }
    catch (Exception ex)
    {
        app.Logger.LogWarning(ex, "Warm-up ไม่สำเร็จ (ข้ามไป)");
    }
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "UBIS Access Service API v1");
        c.RoutePrefix = "BeAccess";
        c.DocumentTitle = "UBIS - Access Service";
    });
}

if (!app.Environment.IsDevelopment())
    app.UseHttpsRedirection();

app.UseCors("AllowFrontend");
app.UseMiddleware<ApiKeyMiddleware>();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();