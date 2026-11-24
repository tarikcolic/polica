using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Market.Infrastructure.Migrations
{
    /// <summary>
    /// Polica (#7): creates the Cities lookup table (supporting entity) and the Pharmacies table (counted entity).
    /// </summary>
    /// <remarks>
    /// Generated with <c>dotnet ef migrations add AddCitiesAndPharmacies</c> after CityEntity, PharmacyEntity,
    /// their EF configurations and DbSets were added.
    /// <list type="bullet">
    /// <item>Cities is created first, because Pharmacies has a foreign key to it (FK_Pharmacies_Cities_CityId,
    /// ReferentialAction.Restrict: a city used by a pharmacy cannot be deleted).</item>
    /// <item>InsertData: the 8 cities from StaticDataSeeder.SeedCities, in every environment. Demo pharmacies are
    /// not here (DynamicDataSeeder).</item>
    /// <item>IX_Pharmacies_Code: filtered unique index (unique among rows that are not soft-deleted), the last guard
    /// behind the duplicate-code check in the handlers. IX_Pharmacies_CityId speeds up the city filter and the join.</item>
    /// </list>
    /// Down() drops Pharmacies first (it depends on Cities), then Cities.
    /// </remarks>
    public partial class AddCitiesAndPharmacies : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Cities",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cities", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Pharmacies",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Code = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Address = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Phone = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    CityId = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Pharmacies", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Pharmacies_Cities_CityId",
                        column: x => x.CityId,
                        principalTable: "Cities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "Cities",
                columns: new[] { "Id", "CreatedAtUtc", "IsDeleted", "ModifiedAtUtc", "Name" },
                values: new object[,]
                {
                    { 1, new DateTime(2026, 11, 23, 0, 0, 0, 0, DateTimeKind.Utc), false, null, "Sarajevo" },
                    { 2, new DateTime(2026, 11, 23, 0, 0, 0, 0, DateTimeKind.Utc), false, null, "Banja Luka" },
                    { 3, new DateTime(2026, 11, 23, 0, 0, 0, 0, DateTimeKind.Utc), false, null, "Tuzla" },
                    { 4, new DateTime(2026, 11, 23, 0, 0, 0, 0, DateTimeKind.Utc), false, null, "Zenica" },
                    { 5, new DateTime(2026, 11, 23, 0, 0, 0, 0, DateTimeKind.Utc), false, null, "Mostar" },
                    { 6, new DateTime(2026, 11, 23, 0, 0, 0, 0, DateTimeKind.Utc), false, null, "Bihać" },
                    { 7, new DateTime(2026, 11, 23, 0, 0, 0, 0, DateTimeKind.Utc), false, null, "Bijeljina" },
                    { 8, new DateTime(2026, 11, 23, 0, 0, 0, 0, DateTimeKind.Utc), false, null, "Brčko" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Cities_Name",
                table: "Cities",
                column: "Name",
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_Pharmacies_CityId",
                table: "Pharmacies",
                column: "CityId");

            migrationBuilder.CreateIndex(
                name: "IX_Pharmacies_Code",
                table: "Pharmacies",
                column: "Code",
                unique: true,
                filter: "[IsDeleted] = 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Pharmacies");

            migrationBuilder.DropTable(
                name: "Cities");
        }
    }
}
