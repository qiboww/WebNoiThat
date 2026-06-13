using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BanNoiThat.Migrations
{
    /// <inheritdoc />
    public partial class AddModel3DUrl : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Model3DUrl",
                table: "Products",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Model3DUrl",
                table: "Products");
        }
    }
}
