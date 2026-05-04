using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using PngCardGenerator.Api.Data;

namespace PngCardGenerator.Api.Migrations;

[DbContext(typeof(CardGeneratorDbContext))]
partial class CardGeneratorDbContextModelSnapshot : ModelSnapshot
{
    protected override void BuildModel(ModelBuilder modelBuilder)
    {
#pragma warning disable 612, 618
        modelBuilder
            .HasAnnotation("ProductVersion", "8.0.4")
            .HasAnnotation("Relational:MaxIdentifierLength", 63);

        NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

        modelBuilder.Entity("PngCardGenerator.Api.Models.CardTemplate", b =>
        {
            b.Property<Guid>("Id")
                .ValueGeneratedOnAdd()
                .HasColumnType("uuid");

            b.Property<DateTime>("CreatedAtUtc")
                .HasColumnType("timestamp with time zone");

            b.Property<int>("Height")
                .HasColumnType("integer");

            b.Property<string>("Name")
                .IsRequired()
                .HasMaxLength(180)
                .HasColumnType("character varying(180)");

            b.Property<DateTime>("UpdatedAtUtc")
                .HasColumnType("timestamp with time zone");

            b.Property<int>("Width")
                .HasColumnType("integer");

            b.HasKey("Id");

            b.ToTable("card_templates", (string?)null);
        });

        modelBuilder.Entity("PngCardGenerator.Api.Models.CardTemplateLayer", b =>
        {
            b.Property<Guid>("Id")
                .ValueGeneratedOnAdd()
                .HasColumnType("uuid");

            b.Property<bool>("Bold")
                .HasColumnType("boolean");

            b.Property<Guid>("CardTemplateId")
                .HasColumnType("uuid");

            b.Property<string>("ClientLayerId")
                .IsRequired()
                .HasMaxLength(120)
                .HasColumnType("character varying(120)");

            b.Property<string>("ColorHex")
                .HasMaxLength(32)
                .HasColumnType("character varying(32)");

            b.Property<string>("FontName")
                .HasMaxLength(120)
                .HasColumnType("character varying(120)");

            b.Property<float>("FontSize")
                .HasColumnType("real");

            b.Property<float>("Height")
                .HasColumnType("real");

            b.Property<string>("Name")
                .IsRequired()
                .HasMaxLength(180)
                .HasColumnType("character varying(180)");

            b.Property<float>("Opacity")
                .HasColumnType("real");

            b.Property<float>("Rotation")
                .HasColumnType("real");

            b.Property<string>("StrokeColorHex")
                .HasMaxLength(32)
                .HasColumnType("character varying(32)");

            b.Property<float>("StrokeThickness")
                .HasColumnType("real");

            b.Property<Guid?>("TemplateAssetId")
                .HasColumnType("uuid");

            b.Property<string>("Text")
                .HasColumnType("text");

            b.Property<string>("TextAlign")
                .IsRequired()
                .HasMaxLength(24)
                .HasColumnType("character varying(24)");

            b.Property<string>("Type")
                .IsRequired()
                .HasMaxLength(24)
                .HasColumnType("character varying(24)");

            b.Property<float>("Width")
                .HasColumnType("real");

            b.Property<float>("X")
                .HasColumnType("real");

            b.Property<float>("Y")
                .HasColumnType("real");

            b.Property<int>("ZIndex")
                .HasColumnType("integer");

            b.HasKey("Id");

            b.HasIndex("CardTemplateId", "ClientLayerId")
                .IsUnique();

            b.HasIndex("TemplateAssetId");

            b.ToTable("card_template_layers", (string?)null);
        });

        modelBuilder.Entity("PngCardGenerator.Api.Models.GeneratedCard", b =>
        {
            b.Property<Guid>("Id")
                .ValueGeneratedOnAdd()
                .HasColumnType("uuid");

            b.Property<Guid?>("CardTemplateId")
                .HasColumnType("uuid");

            b.Property<DateTime>("CreatedAtUtc")
                .HasColumnType("timestamp with time zone");

            b.Property<int>("Height")
                .HasColumnType("integer");

            b.Property<int>("LayerCount")
                .HasColumnType("integer");

            b.Property<string>("Name")
                .IsRequired()
                .HasMaxLength(180)
                .HasColumnType("character varying(180)");

            b.Property<string>("RequestSnapshotJson")
                .IsRequired()
                .HasColumnType("text");

            b.Property<int>("Width")
                .HasColumnType("integer");

            b.HasKey("Id");

            b.HasIndex("CardTemplateId");

            b.ToTable("generated_cards", (string?)null);
        });

        modelBuilder.Entity("PngCardGenerator.Api.Models.TemplateAsset", b =>
        {
            b.Property<Guid>("Id")
                .ValueGeneratedOnAdd()
                .HasColumnType("uuid");

            b.Property<string>("Base64Data")
                .IsRequired()
                .HasColumnType("text");

            b.Property<DateTime>("CreatedAtUtc")
                .HasColumnType("timestamp with time zone");

            b.Property<string>("MimeType")
                .IsRequired()
                .HasMaxLength(80)
                .HasColumnType("character varying(80)");

            b.Property<string>("Sha256")
                .IsRequired()
                .HasMaxLength(64)
                .HasColumnType("character varying(64)");

            b.HasKey("Id");

            b.HasIndex("Sha256")
                .IsUnique();

            b.ToTable("template_assets", (string?)null);
        });

        modelBuilder.Entity("PngCardGenerator.Api.Models.CardTemplateLayer", b =>
        {
            b.HasOne("PngCardGenerator.Api.Models.CardTemplate", "Template")
                .WithMany("Layers")
                .HasForeignKey("CardTemplateId")
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();

            b.HasOne("PngCardGenerator.Api.Models.TemplateAsset", "Asset")
                .WithMany("Layers")
                .HasForeignKey("TemplateAssetId")
                .OnDelete(DeleteBehavior.SetNull);

            b.Navigation("Asset");

            b.Navigation("Template");
        });

        modelBuilder.Entity("PngCardGenerator.Api.Models.GeneratedCard", b =>
        {
            b.HasOne("PngCardGenerator.Api.Models.CardTemplate", "Template")
                .WithMany()
                .HasForeignKey("CardTemplateId")
                .OnDelete(DeleteBehavior.SetNull);

            b.Navigation("Template");
        });

        modelBuilder.Entity("PngCardGenerator.Api.Models.CardTemplate", b =>
        {
            b.Navigation("Layers");
        });

        modelBuilder.Entity("PngCardGenerator.Api.Models.TemplateAsset", b =>
        {
            b.Navigation("Layers");
        });
#pragma warning restore 612, 618
    }
}
