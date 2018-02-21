using EliteJsonApi.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EliteJsonApi.Data
{
    public class EliteDbContext : DbContext
    {
        public EliteDbContext(DbContextOptions<EliteDbContext> options) : base(options) { }

        public DbSet<Belt> Belt { get; set; }
        public DbSet<Body> Body { get; set; }
        public DbSet<Material> Material { get; set; }
        public DbSet<MinorFaction> MinorFaction { get; set; }
        public DbSet<MinorFactionPresence> MinorFactionPresence { get; set; }
        public DbSet<RawMaterialShare> RawMaterialShare { get; set; }
        public DbSet<AtmosphereComposite> AtmosphereComposite { get; set; }
        public DbSet<Ring> Ring { get; set; }
        public DbSet<StarSystem> StarSystem { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            SetUpEntity(builder.Entity<StarSystem>());
            SetUpEntity(builder.Entity<MinorFactionPresence>());
            SetUpEntity(builder.Entity<RawMaterialShare>());
            SetUpEntity(builder.Entity<Material>());
            SetUpEntity(builder.Entity<Belt>());
            SetUpEntity(builder.Entity<Body>());
            SetUpEntity(builder.Entity<Material>());
            SetUpEntity(builder.Entity<AtmosphereComposite>());
            SetUpEntity(builder.Entity<Ring>());
            SetUpEntity(builder.Entity<MinorFaction>());
        }

        private void SetUpEntity(EntityTypeBuilder<MinorFaction> entity)
        {
            entity.HasMany(mf => mf.MinorFactionPresences)
                .WithOne(mfp => mfp.MinorFaction)
                .HasForeignKey(mfp => mfp.MinorFactionId);

            entity.HasIndex(mf => mf.Name).IsUnique();
            entity.HasIndex(mf => mf.IsPlayerFaction).IsUnique(false);
        }

        private void SetUpEntity(EntityTypeBuilder<Belt> entity)
        {
            
        }

        private void SetUpEntity(EntityTypeBuilder<StarSystem> entity)
        {
            entity.HasIndex(ss => ss.EdsmId).IsUnique();
            entity.HasIndex(ss => ss.EddbId).IsUnique();
            entity.HasIndex(ss => ss.Name).IsUnique();
            //entity.HasIndex(ss => ss.Name.ToLower()).IsUnique(); // Not sure this is would work for my use case
            entity.HasIndex(ss => ss.NameLower).IsUnique();

            entity.HasIndex(ss => ss.X).IsUnique(false);
            entity.HasIndex(ss => ss.Y).IsUnique(false);
            entity.HasIndex(ss => ss.Z).IsUnique(false);

            entity.HasIndex(ss => ss.Allegiance).IsUnique(false);
            entity.HasIndex(ss => ss.Security).IsUnique(false);
            entity.HasIndex(ss => ss.Reserves).IsUnique(false);
            entity.HasIndex(ss => ss.PrimaryEconomy).IsUnique(false);
            entity.HasIndex(ss => ss.Government).IsUnique(false);
            entity.HasIndex(ss => ss.State).IsUnique(false);
            entity.HasIndex(ss => ss.PowerPlayLeader).IsUnique(false);
            entity.HasIndex(ss => ss.PowerPlayState).IsUnique(false);

            entity.HasIndex(ss => ss.IsPopulated).IsUnique(false);

            entity.HasMany(ss => ss.MinorFactionPresences)
                .WithOne(mfp => mfp.StarSystem)
                .HasForeignKey(m => m.StarSystemId);
        }

        private void SetUpEntity(EntityTypeBuilder<MinorFactionPresence> entity)
        {
            entity.HasKey(mfp => new { mfp.StarSystemId, mfp.MinorFactionId });
            entity.Property(mfp => mfp.StarSystemId).ValueGeneratedNever();
            entity.Property(mfp => mfp.MinorFactionId).ValueGeneratedNever();

            entity.HasOne(mfp => mfp.StarSystem)
                .WithMany(ss => ss.MinorFactionPresences)
                .HasForeignKey(mfp => mfp.StarSystemId);

            entity.HasOne(mfp => mfp.MinorFaction)
                .WithMany(mf => mf.MinorFactionPresences)
                .HasForeignKey(mfp => mfp.MinorFactionId);
        }

        private void SetUpEntity(EntityTypeBuilder<RawMaterialShare> entity)
        {
            entity.HasKey(rms => new { rms.BodyId, rms.MaterialId });
            entity.HasIndex(rms => rms.Share);
        }

        private void SetUpEntity(EntityTypeBuilder<Material> entity)
        {
            entity.HasIndex(m => m.Name).IsUnique();
            entity.HasIndex(m => m.Type).IsUnique(false);
        }

        private void SetUpEntity(EntityTypeBuilder<Body> entity)
        {
            entity.HasIndex(b => b.Name).IsUnique(false);
            entity.HasIndex(b => b.EdsmId).IsUnique();
            entity.HasIndex(b => b.StarSystemId).IsUnique(false);
            entity.HasIndex(b => b.DistanceToArrival).IsUnique(false);
            entity.HasIndex(b => b.Type).IsUnique(false);
            entity.HasIndex(b => b.SubType).IsUnique(false);
            entity.HasIndex(b => b.SpectralClass).IsUnique(false);
            entity.HasIndex(b => b.IsMainStar).IsUnique(false);
            entity.HasIndex(b => b.IsScoopable).IsUnique(false);
            entity.HasIndex(b => b.IsLandable).IsUnique(false);
            entity.HasIndex(b => b.StarSystemId).IsUnique(false);
        }

        private void SetUpEntity(EntityTypeBuilder<AtmosphereComposite> entity)
        {
            entity.HasKey(ac => new { ac.Component, ac.BodyId });
        }

        private void SetUpEntity(EntityTypeBuilder<Ring> entity)
        {
            entity.HasKey(r => new { r.BodyId, r.Name });
            entity.HasIndex(r => r.Type);
        }
    }
}
