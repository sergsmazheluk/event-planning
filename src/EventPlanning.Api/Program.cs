using AutoMapper;
using EventPlanning.Api.Middleware;
using EventPlanning.Application.Mappings;
using EventPlanning.Infrastructure.Identity;
using EventPlanning.Infrastructure.Notifications.Email;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace EventPlanning.Api
{
    public partial class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddSingleton<IMapper>(sp =>
            {
                // 1) собираем конфигурацию через выражение
                var expr = new MapperConfigurationExpression();
                expr.AddProfile<EventProfile>();

                // 2) достаЄм ILoggerFactory из DI Ч нужен дл€ некоторых версий AutoMapper
                var loggerFactory = sp.GetRequiredService<ILoggerFactory>();

                // 3) создаЄм конфигурацию и сам маппер
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

            builder.Services.AddSingleton<IEmailSender, SmtpEmailSender>();
            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

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


