using EventPlanning.Domain.Events;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System.Text.Json;

namespace EventPlanning.Infrastructure.Identity
{
    public sealed class AppDbContext : IdentityDbContext<ApplicationUser>
    {
        public DbSet<EventDefinition> EventDefinitions => Set<EventDefinition>();
        public DbSet<Event> Events => Set<Event>();
        public DbSet<Registration> Registrations => Set<Registration>();

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder b)
        {
            base.OnModelCreating(b);

            var jsonOpts = new JsonSerializerOptions(JsonSerializerDefaults.Web);

            var fieldsConverter = new ValueConverter<List<FieldDefinition>, string>(
                v => JsonSerializer.Serialize(v ?? new(), jsonOpts),
                v => JsonSerializer.Deserialize<List<FieldDefinition>>(v ?? "[]", jsonOpts) ?? new()
            );

            b.Entity<EventDefinition>(e =>
            {
                e.ToTable("EventDefinitions");
                e.HasKey(x => x.Id);
                e.Property(x => x.Name).IsRequired();
                e.Property(x => x.Fields)
                 .HasConversion(fieldsConverter)
                 .HasColumnName("FieldsJson")
                 .HasColumnType("nvarchar(max)");
            });

            b.Entity<Event>(e =>
            {
                e.ToTable("Events");
                e.HasKey(x => x.Id);
                e.Property(x => x.CustomDataJson).HasColumnType("nvarchar(max)");
                e.Property(x => x.RegistrationsCount).HasDefaultValue(0);
            });

            b.Entity<Registration>(e =>
            {
                e.ToTable("Registrations");
                e.HasKey(x => x.Id);
                e.Property(x => x.AnswersJson).HasColumnType("nvarchar(max)");
            });
        }
    }
}