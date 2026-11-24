using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Market.Infrastructure.Migrations
{
    /// <summary>
    /// Polica (#6): creates the three code list tables the chain administrator maintains:
    /// Manufacturers, DosageForms and WriteOffReasons (supporting entities, not counted toward the 10).
    /// </summary>
    /// <remarks>
    /// Generated with <c>dotnet ef migrations add AddCodeLists</c> after the entities, their EF configurations
    /// (Configurations/CodeLists) and their DbSets were added. EF compares the model with
    /// DatabaseContextModelSnapshot.cs and writes the difference as CreateTable / InsertData / CreateIndex.
    /// <list type="bullet">
    /// <item>InsertData: the reference rows from StaticDataSeeder (5 dosage forms, 5 write-off reasons), so every
    /// environment, production included, starts with them. Demo manufacturers are not here (DynamicDataSeeder).</item>
    /// <item>CreateIndex with <c>filter: "[IsDeleted] = 0"</c>: a filtered unique index, i.e. the name is unique
    /// only among rows that are not soft-deleted, so a deleted name can be used again.</item>
    /// </list>
    /// The API applies this migration on startup (DatabaseInitializer → MigrateAsync). Down() drops the tables.
    /// </remarks>
    public partial class AddCodeLists : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DosageForms",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DosageForms", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Manufacturers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Country = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Manufacturers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "WriteOffReasons",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    RequiresNote = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WriteOffReasons", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "DosageForms",
                columns: new[] { "Id", "CreatedAtUtc", "IsDeleted", "ModifiedAtUtc", "Name" },
                values: new object[,]
                {
                    { 1, new DateTime(2026, 11, 23, 0, 0, 0, 0, DateTimeKind.Utc), false, null, "Tableta" },
                    { 2, new DateTime(2026, 11, 23, 0, 0, 0, 0, DateTimeKind.Utc), false, null, "Kapsula" },
                    { 3, new DateTime(2026, 11, 23, 0, 0, 0, 0, DateTimeKind.Utc), false, null, "Sirup" },
                    { 4, new DateTime(2026, 11, 23, 0, 0, 0, 0, DateTimeKind.Utc), false, null, "Mast" },
                    { 5, new DateTime(2026, 11, 23, 0, 0, 0, 0, DateTimeKind.Utc), false, null, "Injekcija" }
                });

            migrationBuilder.InsertData(
                table: "WriteOffReasons",
                columns: new[] { "Id", "CreatedAtUtc", "IsDeleted", "ModifiedAtUtc", "Name", "RequiresNote" },
                values: new object[,]
                {
                    { 1, new DateTime(2026, 11, 23, 0, 0, 0, 0, DateTimeKind.Utc), false, null, "Istekao rok trajanja", false },
                    { 2, new DateTime(2026, 11, 23, 0, 0, 0, 0, DateTimeKind.Utc), false, null, "Oštećeno pakovanje", false },
                    { 3, new DateTime(2026, 11, 23, 0, 0, 0, 0, DateTimeKind.Utc), false, null, "Povučena serija", false },
                    { 4, new DateTime(2026, 11, 23, 0, 0, 0, 0, DateTimeKind.Utc), false, null, "Neodgovarajuće čuvanje", true },
                    { 5, new DateTime(2026, 11, 23, 0, 0, 0, 0, DateTimeKind.Utc), false, null, "Ostalo", true }
                });

            migrationBuilder.CreateIndex(
                name: "IX_DosageForms_Name",
                table: "DosageForms",
                column: "Name",
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_Manufacturers_Name",
                table: "Manufacturers",
                column: "Name",
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_WriteOffReasons_Name",
                table: "WriteOffReasons",
                column: "Name",
                unique: true,
                filter: "[IsDeleted] = 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DosageForms");

            migrationBuilder.DropTable(
                name: "Manufacturers");

            migrationBuilder.DropTable(
                name: "WriteOffReasons");
        }
    }
}
