using Microsoft.EntityFrameworkCore;
using PngCardGenerator.Api.Models;

namespace PngCardGenerator.Api.Data;

public sealed class CardGeneratorDbContext : DbContext
{
    public CardGeneratorDbContext(DbContextOptions<CardGeneratorDbContext> options)
        : base(options)
    {
    }

    public DbSet<CardTemplate> CardTemplates => Set<CardTemplate>();
    public DbSet<CardTemplateLayer> CardTemplateLayers => Set<CardTemplateLayer>();
    public DbSet<TemplateAsset> TemplateAssets => Set<TemplateAsset>();
    public DbSet<GeneratedCard> GeneratedCards => Set<GeneratedCard>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<CardTemplate>(entity =>
        {
            entity.ToTable("card_templates");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Name).HasMaxLength(180).IsRequired();
            entity.Property(x => x.CreatedAtUtc).IsRequired();
            entity.Property(x => x.UpdatedAtUtc).IsRequired();
            entity.HasMany(x => x.Layers)
                .WithOne(x => x.Template)
                .HasForeignKey(x => x.CardTemplateId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<CardTemplateLayer>(entity =>
        {
            entity.ToTable("card_template_layers");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.ClientLayerId).HasMaxLength(120).IsRequired();
            entity.Property(x => x.Type).HasConversion<string>().HasMaxLength(24).IsRequired();
            entity.Property(x => x.Name).HasMaxLength(180).IsRequired();
            entity.Property(x => x.Text).HasColumnType("text");
            entity.Property(x => x.FontName).HasMaxLength(120);
            entity.Property(x => x.ColorHex).HasMaxLength(32);
            entity.Property(x => x.StrokeColorHex).HasMaxLength(32);
            entity.Property(x => x.TextAlign).HasConversion<string>().HasMaxLength(24).IsRequired();
            entity.HasIndex(x => new { x.CardTemplateId, x.ClientLayerId }).IsUnique();
            entity.HasOne(x => x.Asset)
                .WithMany(x => x.Layers)
                .HasForeignKey(x => x.TemplateAssetId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<TemplateAsset>(entity =>
        {
            entity.ToTable("template_assets");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.MimeType).HasMaxLength(80).IsRequired();
            entity.Property(x => x.Base64Data).HasColumnType("text").IsRequired();
            entity.Property(x => x.Sha256).HasMaxLength(64).IsRequired();
            entity.Property(x => x.CreatedAtUtc).IsRequired();
            entity.HasIndex(x => x.Sha256).IsUnique();
        });

        modelBuilder.Entity<GeneratedCard>(entity =>
        {
            entity.ToTable("generated_cards");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Name).HasMaxLength(180).IsRequired();
            entity.Property(x => x.RequestSnapshotJson).HasColumnType("text").IsRequired();
            entity.Property(x => x.CreatedAtUtc).IsRequired();
            entity.HasOne(x => x.Template)
                .WithMany()
                .HasForeignKey(x => x.CardTemplateId)
                .OnDelete(DeleteBehavior.SetNull);
        });
    }
}
