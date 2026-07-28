using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Career_Guidance_Platform.Migrations
{
    /// <inheritdoc />
    public partial class RenameQuestionTypeNameToTitle : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Name",
                table: "QuestionTypes",
                newName: "Title");

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "QuestionTypes",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Description",
                table: "QuestionTypes");

            migrationBuilder.RenameColumn(
                name: "Title",
                table: "QuestionTypes",
                newName: "Name");
        }
    }
}
