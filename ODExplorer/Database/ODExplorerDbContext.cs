using Microsoft.EntityFrameworkCore;
using ODExplorer.Database.DTOs;
using ODUtils.Database.Base;
using ODUtils.Spansh;

namespace ODExplorer.Database
{
    public sealed class ODExplorerDbContext(DbContextOptions options) : ODDbContextBase(options)
    {
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
        public DbSet<CartoIgnoredSystemsDTO> CartoIgnoredSystems { get; set; }
        public DbSet<EdAstroPoiDTO> EdAstroPois { get; set; }
        public DbSet<SpanshCsvDTO> SpanshCsvs { get; set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

        public override void OnEfCoreModelCreating(ModelBuilder modelBuilder)
        {
            base.OnEfCoreModelCreating(modelBuilder);

            modelBuilder.Entity<CartoIgnoredSystemsDTO>()
                .HasMany(e => e.Commanders)
                .WithMany()
                .UsingEntity("CommanderIgnoredSystems");

            modelBuilder.Entity<EdAstroPoiDTO>()
           .HasKey(c => c.Id);

            modelBuilder.Entity<EdAstroPoiDTO>()
                .Property(c => c.Id)
                .ValueGeneratedNever();

            modelBuilder.Entity<SpanshCsvDTO>().HasKey(u => new
            {
                u.CsvType,
                u.CommanderID
            });
        }
    }
}
