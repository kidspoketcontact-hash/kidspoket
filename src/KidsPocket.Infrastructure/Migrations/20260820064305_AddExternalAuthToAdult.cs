using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KidsPocket.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddExternalAuthToAdult : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "PasswordHash",
                table: "Adults",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<string>(
                name: "ExternalId",
                table: "Adults",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Provider",
                table: "Adults",
                type: "integer",
                nullable: false,
                defaultValue: 1); // AuthProvider.Local - כל ה-Adults הקיימים הם הרשמה מקומית

            migrationBuilder.CreateIndex(
                name: "IX_Adults_Provider_ExternalId",
                table: "Adults",
                columns: new[] { "Provider", "ExternalId" },
                unique: true,
                filter: "\"ExternalId\" IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Adults_Provider_ExternalId",
                table: "Adults");

            migrationBuilder.DropColumn(
                name: "ExternalId",
                table: "Adults");

            migrationBuilder.DropColumn(
                name: "Provider",
                table: "Adults");

            migrationBuilder.AlterColumn<string>(
                name: "PasswordHash",
                table: "Adults",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);
        }
    }
}
