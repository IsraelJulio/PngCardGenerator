using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace PngCardGenerator.Api.Migrations;

public partial class AddTemplatePersistence : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "card_templates",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                Name = table.Column<string>(type: "character varying(180)", maxLength: 180, nullable: false),
                Width = table.Column<int>(type: "integer", nullable: false),
                Height = table.Column<int>(type: "integer", nullable: false),
                CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_card_templates", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "template_assets",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                MimeType = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                Base64Data = table.Column<string>(type: "text", nullable: false),
                Sha256 = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_template_assets", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "generated_cards",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                CardTemplateId = table.Column<Guid>(type: "uuid", nullable: true),
                Name = table.Column<string>(type: "character varying(180)", maxLength: 180, nullable: false),
                Width = table.Column<int>(type: "integer", nullable: false),
                Height = table.Column<int>(type: "integer", nullable: false),
                LayerCount = table.Column<int>(type: "integer", nullable: false),
                RequestSnapshotJson = table.Column<string>(type: "text", nullable: false),
                CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_generated_cards", x => x.Id);
                table.ForeignKey(
                    name: "FK_generated_cards_card_templates_CardTemplateId",
                    column: x => x.CardTemplateId,
                    principalTable: "card_templates",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.SetNull);
            });

        migrationBuilder.CreateTable(
            name: "card_template_layers",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                CardTemplateId = table.Column<Guid>(type: "uuid", nullable: false),
                ClientLayerId = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                Type = table.Column<string>(type: "character varying(24)", maxLength: 24, nullable: false),
                Name = table.Column<string>(type: "character varying(180)", maxLength: 180, nullable: false),
                TemplateAssetId = table.Column<Guid>(type: "uuid", nullable: true),
                Text = table.Column<string>(type: "text", nullable: true),
                X = table.Column<float>(type: "real", nullable: false),
                Y = table.Column<float>(type: "real", nullable: false),
                Width = table.Column<float>(type: "real", nullable: false),
                Height = table.Column<float>(type: "real", nullable: false),
                Rotation = table.Column<float>(type: "real", nullable: false),
                Opacity = table.Column<float>(type: "real", nullable: false),
                FontName = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                FontSize = table.Column<float>(type: "real", nullable: false),
                ColorHex = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                StrokeColorHex = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                StrokeThickness = table.Column<float>(type: "real", nullable: false),
                Bold = table.Column<bool>(type: "boolean", nullable: false),
                TextAlign = table.Column<string>(type: "character varying(24)", maxLength: 24, nullable: false),
                ZIndex = table.Column<int>(type: "integer", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_card_template_layers", x => x.Id);
                table.ForeignKey(
                    name: "FK_card_template_layers_card_templates_CardTemplateId",
                    column: x => x.CardTemplateId,
                    principalTable: "card_templates",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_card_template_layers_template_assets_TemplateAssetId",
                    column: x => x.TemplateAssetId,
                    principalTable: "template_assets",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.SetNull);
            });

        migrationBuilder.CreateIndex(
            name: "IX_card_template_layers_CardTemplateId_ClientLayerId",
            table: "card_template_layers",
            columns: new[] { "CardTemplateId", "ClientLayerId" },
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_card_template_layers_TemplateAssetId",
            table: "card_template_layers",
            column: "TemplateAssetId");

        migrationBuilder.CreateIndex(
            name: "IX_generated_cards_CardTemplateId",
            table: "generated_cards",
            column: "CardTemplateId");

        migrationBuilder.CreateIndex(
            name: "IX_template_assets_Sha256",
            table: "template_assets",
            column: "Sha256",
            unique: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "card_template_layers");
        migrationBuilder.DropTable(name: "generated_cards");
        migrationBuilder.DropTable(name: "template_assets");
        migrationBuilder.DropTable(name: "card_templates");
    }
}
