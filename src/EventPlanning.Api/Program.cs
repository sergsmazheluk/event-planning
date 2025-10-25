using AutoMapper;
using EventPlanning.Api.Middleware;
using EventPlanning.Application.Mappings;
using EventPlanning.Application.Validation;
using EventPlanning.Infrastructure.Auth;
using EventPlanning.Infrastructure.Identity;
using EventPlanning.Infrastructure.Notifications.Email;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace EventPlanning.Api
{
    public partial class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // JWT options
            builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection(JwtOptions.SectionName));
            var jwt = builder.Configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>()!;

            // Auth
            builder.Services
                .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(opt =>
                {
                    opt.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidIssuer = jwt.Issuer,
                        ValidAudience = jwt.Audience,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Key)),
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateIssuerSigningKey = true,
                        ValidateLifetime = true,
                        ClockSkew = TimeSpan.FromSeconds(15)
                    };
                });

            builder.Services.AddSingleton<IMapper>(sp =>
            {
                // 1) собираем конфигурацию через выражение
                var expr = new MapperConfigurationExpression();
                expr.AddProfile<EventProfile>();

                // 2) достаём ILoggerFactory из DI — нужен для некоторых версий AutoMapper
                var loggerFactory = sp.GetRequiredService<ILoggerFactory>();

                // 3) создаём конфигурацию и сам маппер
                var config = new MapperConfiguration(expr, loggerFactory);
                return new Mapper(config, sp.GetService);
            });

            builder.Services.AddDbContext<AppDbContext>(o =>
                o.UseSqlServer(builder.Configuration.GetConnectionString("Default")));

            builder.Services.AddIdentity<ApplicationUser, IdentityRole>(opt =>
            {
                opt.SignIn.RequireConfirmedEmail = true;
            })
                .AddEntityFrameworkStores<AppDbContext>()
                .AddDefaultTokenProviders();

            builder.Services.AddSingleton<IRegistrationAnswersValidator, RegistrationAnswersValidator>();

            if (builder.Environment.IsDevelopment())
            {
                builder.Services.AddSingleton<IEmailSender, NoopEmailSender>();   // ← dev
            }
            else
            {
                builder.Services.AddSingleton<IEmailSender, SmtpEmailSender>();   // ← prod
            }

            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            // JWT token service
            builder.Services.AddSingleton<IJwtTokenService, JwtTokenService>();

            // Swagger + Bearer
            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new() { Title = "EventPlanning API", Version = "v1" });
                c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT",
                    In = Microsoft.OpenApi.Models.ParameterLocation.Header,
                    Description = "Введите: Bearer {ваш_токен}"
                });
                c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
                {
            {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }},
            Array.Empty<string>()}});
            });

            var app = builder.Build();
            app.UseSwagger();
            app.UseSwaggerUI();
            app.UseHttpsRedirection();
            app.UseAuthentication();
            app.UseRequireCompletedProfile();
            app.UseAuthorization();        
            app.MapControllers();
            app.Run();
        }
    }
    public partial class Program { }
}


