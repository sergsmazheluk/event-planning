using EventPlanning.Domain.Events;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using Microsoft.EntityFrameworkCore.Metadata; // для SetValueComparer
using System.Linq;


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

            var fieldsComparer = new ValueComparer<List<FieldDefinition>>((a, b) => 
            JsonSerializer.Serialize(a ?? new List<FieldDefinition>(), jsonOpts) == 
            JsonSerializer.Serialize(b ?? new List<FieldDefinition>(), jsonOpts), 
            v => (v == null ? 0 : JsonSerializer.Serialize(v, jsonOpts).GetHashCode()), 
            v => v == null ? new List<FieldDefinition>() : v.Select(x => new FieldDefinition 
            {
                Key = x.Key,
                Label = x.Label,
                Type = x.Type,
                Required = x.Required,
                Options = x.Options == null ? null : x.Options.ToArray()
            }).ToList());

            b.Entity<EventDefinition>(e =>
            {
                e.ToTable("EventDefinitions");
                e.HasKey(x => x.Id);
                e.Property(x => x.Name).IsRequired();

                var prop = e.Property(x => x.Fields);

                prop.HasConversion(fieldsConverter);              // конвертер JSON
                prop.Metadata.SetValueComparer(fieldsComparer);   // компаратор для коллекции
                prop.HasColumnName("FieldsJson");
                prop.HasColumnType("nvarchar(max)");
            });

            b.Entity<Event>(e =>
            {
                e.ToTable("Events");
                e.HasKey(x => x.Id);
                e.Property(x => x.CustomDataJson).HasColumnType("nvarchar(max)");
                e.Property(x => x.RegistrationsCount).HasDefaultValue(0);
                e.Property(x => x.RowVersion).IsRowVersion();
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