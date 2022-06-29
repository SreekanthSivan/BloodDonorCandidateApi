using Microsoft.EntityFrameworkCore.Migrations;

namespace WebAPI.Migrations
{
    public partial class imageurladded : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "imageUrl",
                table: "DCandidates",
                type: "nvarchar(512)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "location",
                table: "DCandidates",
                type: "nvarchar(200)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "imageUrl",
                table: "DCandidates");

            migrationBuilder.DropColumn(
                name: "location",
                table: "DCandidates");
        }
    }
}
