using Microsoft.EntityFrameworkCore;
using UBIS.HR.Application.Interfaces;
using UBIS.HR.Infrastructure.Services;
using UBIS.HR.Domain.Entities;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using UBIS.HR.Api.Services;
using UBIS.HR.Api.Authorization;
using UBIS.HR.Application.Authorization;
using UBIS.HR.Infrastructure.Data;
using UBIS.HR.Infrastructure.Repositories;
using UBIS.HR.Infrastructure.Repositories.Interfaces;
using UBIS.HR.Api.Middleware;


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<HrDbContext>(options => options.UseNpgsql(builder.Configuration.GetConnectionString("HrDatabase")));

builder.Services.AddScoped<IBranchRepos, BranchRepos>();
builder.Services.AddScoped<IBenefitRepos, BenefitRepos>();
builder.Services.AddScoped<IBenefitPlanRepos, BenefitPlanRepos>();
builder.Services.AddScoped<IBenefitPlanItemRepos, BenefitPlanItemRepos>();
builder.Services.AddScoped<IEmployeeTypeRepos, EmployeeTypeRepos>();
builder.Services.AddScoped<IEmployeeRepos, EmployeeRepos>();
builder.Services.AddScoped<ICompanyRepos, CompanyRepos>();
builder.Services.AddScoped<IOrganizationUnitRepos, OrganizationUnitRepos>();
builder.Services.AddScoped<IOrganizationLevelTypeRepos, OrganizationLevelTypeRepos>();
builder.Services.AddScoped<IPodAdminBranchRepos, PodAdminBranchRepos>();
builder.Services.AddScoped<IPositionRepos, PositionRepos>();
builder.Services.AddScoped<IPositionLevelRepos, PositionLevelRepos>();
builder.Services.AddScoped<IEmployeeBenefitPlanRepos, EmployeeBenefitPlanRepos>();
builder.Services.AddScoped<IPrettyCashRepos, PrettyCashRepos>();
builder.Services.AddScoped<IPrefixRepos, PrefixRepos>();
builder.Services.AddScoped<IAttachmentRepos, AttachmentRepos>();
builder.Services.AddScoped<IReasonApproveRepos, ReasonApproveRepos>();
builder.Services.AddScoped<IApprovalRepos, ApprovalRepos>();
builder.Services.AddScoped<IRouteApproveRepos, RouteApproveRepos>();

builder.Services.AddHttpClient<IAccessServiceClient, AccessServiceClient>();
builder.Services.AddScoped<IAttachmentService, AttachmentService>();
builder.Services.AddScoped<IApprovalRouteResolverService, ApprovalRouteResolverService>();
builder.Services.AddScoped<IRouteApproveService, RouteApproveService>();
builder.Services.AddScoped<IOrganizationLevelTypeService, OrganizationLevelTypeService>();
builder.Services.AddScoped<IPositionService, PositionService>();
builder.Services.AddScoped<IPositionLevelService, PositionLevelService>();
builder.Services.AddScoped<IOrganizationUnitService, OrganizationUnitService>();
builder.Services.AddScoped<IEmployeeService, EmployeeService>();
builder.Services.AddScoped<IEmployeeTypeService, EmployeeTypeService>();
builder.Services.AddScoped<IBenefitService, BenefitService>();
builder.Services.AddScoped<IBenefitPlanService, BenefitPlanService>();
builder.Services.AddScoped<IBenefitPlanItemService, BenefitPlanItemService>();
builder.Services.AddScoped<ICompanyService, CompanyService>();
builder.Services.AddScoped<IBranchService, BranchService>();
builder.Services.AddScoped<IPodAdminBranchService, PodAdminBranchService>();
builder.Services.AddScoped<PrettyCashService>();
builder.Services.AddScoped<IPrettyCashService>(sp => sp.GetRequiredService<PrettyCashService>());
builder.Services.AddScoped<IApprovalDocumentService>(sp => sp.GetRequiredService<PrettyCashService>());

builder.Services.AddScoped<IApprovalService, ApprovalService>();
builder.Services.AddScoped<IDocNumberService, DocNumberService>();

var jwtSecret = builder.Configuration["Jwt:Secret"]
    ?? throw new InvalidOperationException("Jwt:Secret is not configured.");

builder.Services.AddAuthentication()
    .AddJwtBearer("EntraID", options =>
    {
        options.Authority =
            $"https://login.microsoftonline.com/{builder.Configuration["Azure:TenantId"]}/v2.0";

        options.Audience = builder.Configuration["Azure:ClientId"];

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true
        };
    })
    .AddJwtBearer("Local", options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey =
                new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(jwtSecret)
                )
        };
    });

builder.Services.AddSingleton<IAuthorizationHandler, PermissionHandler>();
builder.Services.AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>();
builder.Services.AddAuthorization(options =>
{
    options.DefaultPolicy = new AuthorizationPolicyBuilder(
        // "EntraID",
        "Local"
    )
    .RequireAuthenticatedUser()
    .Build();

    // ไม่ต้องลงทะเบียน Policy ทีละตัวอีกต่อไป
    // PermissionPolicyProvider จะสร้างให้อัตโนมัติจากชื่อใน [Authorize(Policy = "...")]
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
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "UBIS HR Service API v1");
        c.RoutePrefix = "BeHr";
        c.DocumentTitle = "UBIS - HR Service";
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