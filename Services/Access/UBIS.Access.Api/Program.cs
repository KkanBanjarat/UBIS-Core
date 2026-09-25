using Microsoft.EntityFrameworkCore;
using UBIS.Access.Application.Interfaces;
using UBIS.Access.Infrastructure.Services;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authentication.JwtBearer;
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
builder.Services.AddScoped<IPermissionService, PermissionService>();
builder.Services.AddScoped<IRoleService, RoleService>();
builder.Services.AddScoped<IRolePermissionService, RolePermissionService>();
builder.Services.AddScoped<IUserRoleService, UserRoleService>();

builder.Services.AddAuthentication().AddJwtBearer("EntraID", options =>
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
        OnAuthenticationFailed = context =>
        {
            return Task.CompletedTask;
        },
        OnTokenValidated = context =>
        {
            return Task.CompletedTask;
        }
    };
});

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:5173")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

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

app.UseHttpsRedirection();
app.UseCors("AllowFrontend");
// ตรวจ API Key ก่อน Authentication — ถ้าไม่มี Key ก็ไม่ต้องเสียเวลา Validate Token
app.UseMiddleware<ApiKeyMiddleware>();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();