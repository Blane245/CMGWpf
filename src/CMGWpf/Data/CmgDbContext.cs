using CMGWpf.Model.Database;
using Microsoft.EntityFrameworkCore;

namespace CMGWpf.Data
{
    public class CmgDbContext : DbContext
    {
        public CmgDbContext()
        {
        }

        public CmgDbContext(DbContextOptions<CmgDbContext> options) : base(options)
        {
        }

        // DbSets for your entities
        public DbSet<Ensemble> Ensembles { get; set; }
        public DbSet<Voice> Voices { get; set; }
        public DbSet<Tag> Tags { get; set; }
        public DbSet<NoteSequence> NoteSequences { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                // Default connection string - matches your appsettings.json
                var connectionString = "Server=localhost;Port=3306;Database=cmg;Uid=root;Pwd=;";

                optionsBuilder.UseMySql(
                    connectionString,
                    ServerVersion.AutoDetect(connectionString),
                    options => options
                        .EnableRetryOnFailure(
                            maxRetryCount: 5,
                            maxRetryDelay: TimeSpan.FromSeconds(10),
                            errorNumbersToAdd: null)
                );

                // Enable sensitive data logging in debug mode
#if DEBUG
                optionsBuilder.EnableSensitiveDataLogging();
                optionsBuilder.EnableDetailedErrors();
#endif
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure Ensemble entity
            modelBuilder.Entity<Ensemble>(entity =>
            {
                entity.ToTable("ensemble");

                // Primary key
                entity.HasKey(e => e.Name);

                // Properties
                entity.Property(e => e.Name)
                    .HasMaxLength(255)
                    .IsRequired();

                entity.Property(e => e.Description)
                    .HasMaxLength(1000);

                // Many-to-many relationship with Voice
                entity.HasMany(e => e.Voices)
                    .WithMany(v => v.Ensembles)
                    .UsingEntity<Dictionary<string, object>>(
                        "ensemble_voice",
                        j => j
                            .HasOne<Voice>()
                            .WithMany()
                            .HasForeignKey("voice_name")
                            .HasPrincipalKey(nameof(Voice.Name))
                            .OnDelete(DeleteBehavior.Cascade),
                        j => j
                            .HasOne<Ensemble>()
                            .WithMany()
                            .HasForeignKey("ensemble_name")
                            .HasPrincipalKey(nameof(Ensemble.Name))
                            .OnDelete(DeleteBehavior.Cascade),
                        j =>
                        {
                            j.HasKey("ensemble_name", "voice_name");
                            j.ToTable("ensemble_voice");
                        }
                    );
            });

            // Configure Voice entity
            modelBuilder.Entity<Voice>(entity =>
            {
                entity.ToTable("voice");

                // Primary key
                entity.HasKey(v => v.Name);

                // Properties
                entity.Property(v => v.Name)
                    .HasMaxLength(255)
                    .IsRequired();

                entity.Property(v => v.Description)
                    .HasMaxLength(1000);

                entity.Property(v => v.Timbre)
                    .HasConversion<string>()
                    .HasMaxLength(50);

                entity.Property(v => v.RegisterLo)
                    .HasColumnType("float");

                entity.Property(v => v.RegisterHi)
                    .HasColumnType("float");

                entity.Property(v => v.Duration)
                    .HasColumnType("float");

                entity.Property(v => v.SoundFontFile)
                    .HasMaxLength(500);

                entity.Property(v => v.PresetName)
                    .HasMaxLength(255);
            });
            // Configure NoteSequence entity
            modelBuilder.Entity<NoteSequence>(entity =>
            {
                entity.ToTable("notesequence");

                // Primary key
                entity.HasKey(e => new { e.Name });

                // Properties
                entity.Property(e => e.Name)
                    .HasMaxLength(45)
                    .IsRequired();

                entity.Property(e => e.Items);

                // Many-to-many relationship with Tag
                entity.HasMany(e => e.Tags)
                    .WithMany(v => v.NoteSequences)
                    .UsingEntity<Dictionary<string, object>>(
                        "notesequence_tag",
                        j => j
                            .HasOne<Tag>()
                            .WithMany()
                            .HasForeignKey("tag_name")
                            .HasPrincipalKey(nameof(Tag.Name))
                            .OnDelete(DeleteBehavior.Cascade),
                        j => j
                            .HasOne<NoteSequence>()
                            .WithMany()
                            .HasForeignKey("notesequence_name")
                            .HasPrincipalKey(nameof(NoteSequence.Name))
                            .OnDelete(DeleteBehavior.Cascade),
                        j =>
                        {
                            j.HasKey("notesequence_name", "tag_name");
                            j.ToTable("notesequence_tag");
                        }
                    );
            });

            // Configure Tag entity
            modelBuilder.Entity<Tag>(entity =>
            {
                entity.ToTable("tag");

                // Primary key
                entity.HasKey(e => e.Name);

                // Properties
                entity.Property(e => e.Name)
                    .HasMaxLength(45)
                    .IsRequired();

                // Many-to-many relationship with NoteSequence
                entity.HasMany(e => e.NoteSequences)
                    .WithMany(v => v.Tags)
                    .UsingEntity<Dictionary<string, object>>(
                        "notesequence_tag",
                        j => j
                            .HasOne<NoteSequence>()
                            .WithMany()
                            .HasForeignKey("notesequence_name")
                            .HasPrincipalKey(nameof(NoteSequence.Name))
                            .OnDelete(DeleteBehavior.Cascade),
                        j => j
                            .HasOne<Tag>()
                            .WithMany()
                            .HasForeignKey("tag_name")
                            .HasPrincipalKey(nameof(Tag.Name))
                            .OnDelete(DeleteBehavior.Cascade),
                        j =>
                        {
                            j.HasKey("notesequence_name", "tag_name");
                            j.ToTable("notesequence_tag");
                        }
                    );
            });
        }
    }
}
