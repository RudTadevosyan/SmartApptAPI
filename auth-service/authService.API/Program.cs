using System.Text;
using authService.API.CustomMiddlewares;
using authService.Application.Extensions;
using Microsoft.IdentityModel.Tokens;


namespace authService.API;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        
        builder.Configuration.AddEnvironmentVariables();
        // Add all services
        builder.Services.AddApplicationServices(builder.Configuration);

        builder.Services.AddControllers();
        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        builder.Services.AddEndpointsApiExplorer();
        
        // JWT
        var key = Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!);
        builder.Services.AddAuthentication(opt =>
            {
                opt.DefaultAuthenticateScheme = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme;
                opt.DefaultChallengeScheme = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = builder.Configuration["Jwt:Issuer"],
                    ValidAudience = builder.Configuration["Jwt:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(key)
                };
            });
        
        //swagger 
        builder.Services.AddSwaggerGen();
        var app = builder.Build();

        ///// await app.Services.ApplyMigrationsAsync(); For the Docker
        
        await app.Services.SeedRolesAsync();
        
        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }
        else
        {
            app.UseHttpsRedirection();
        }
        
        app.UseCookiePolicy(new CookiePolicyOptions
        {
            Secure = app.Environment.IsDevelopment() ? CookieSecurePolicy.SameAsRequest : CookieSecurePolicy.Always,
            MinimumSameSitePolicy = app.Environment.IsDevelopment() ? SameSiteMode.Lax : SameSiteMode.None
        });
        
        app.UseMiddleware<GlobalExceptionHandler>();

        app.UseAuthentication();
        app.UseAuthorization();

        app.MapControllers();

        app.MapGet("/health", () => Results.Ok(new { status = "Healthy", service = "AuthService", time = DateTime.UtcNow }));
        
        await app.RunAsync();
    }
}