using BasicAuthDemo;
using BasicAuthDemo.DbContexts;
using BasicAuthDemo.Entities;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddIdentityCore<UserEntity>(opts =>
{
    opts.Password.RequireNonAlphanumeric = false;
    opts.Password.RequireDigit = false;
    opts.Password.RequireLowercase = false;
    opts.Password.RequireUppercase = false;
})
            .AddRoles<UserRole>()
            .AddRoleManager<RoleManager<UserRole>>()
            .AddSignInManager<SignInManager<UserEntity>>()
            .AddRoleValidator<RoleValidator<UserRole>>()
            .AddEntityFrameworkStores<AuthDbContext>();

builder.Services.AddScoped<ITokenService, TokenService>();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(opts =>
    {
        opts.TokenValidationParameters = new()
        {
            ValidateAudience = false, // جمهور
            ValidateIssuer = false,   //
            ValidateLifetime = true,  // 7 days
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(Constants.TokenKey!)),
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization(opts =>
{
    opts.AddPolicy("OnlyAdmin", _ => _.RequireClaim(ClaimTypes.Role, "Admin"));
});


// JWT
// Cookie
// OAuth2 OpenId
// IdentityServer 4

builder.Services.AddDbContext<AuthDbContext>(_ => _.UseSqlite("Data Source=DemoAuth3.db"));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

using var scope = app.Services.CreateScope();
var userManager = scope.ServiceProvider.GetRequiredService<UserManager<UserEntity>>();
var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<UserRole>>();
try
{
    await Seed.SeedUsers(userManager, roleManager);
}
catch (Exception ex)
{
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    logger.LogError(ex, "An error occured");
}

app.Run();
