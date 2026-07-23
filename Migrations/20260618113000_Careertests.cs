using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Career_Guidance_Platform.Migrations
{
    /// <inheritdoc />
    public partial class Careertests : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AssessmentResults_AspNetUsers_UserId1",
                table: "AssessmentResults");

            migrationBuilder.DropIndex(
                name: "IX_AssessmentResults_UserId1",
                table: "AssessmentResults");

            migrationBuilder.DropColumn(
                name: "UserId1",
                table: "AssessmentResults");

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "AssessmentResults",
                type: "varchar(255)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_AssessmentResults_UserId",
                table: "AssessmentResults",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_AssessmentResults_AspNetUsers_UserId",
                table: "AssessmentResults",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AssessmentResults_AspNetUsers_UserId",
                table: "AssessmentResults");

            migrationBuilder.DropIndex(
                name: "IX_AssessmentResults_UserId",
                table: "AssessmentResults");

            migrationBuilder.AlterColumn<int>(
                name: "UserId",
                table: "AssessmentResults",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(255)")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "UserId1",
                table: "AssessmentResults",
                type: "varchar(255)",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_AssessmentResults_UserId1",
                table: "AssessmentResults",
                column: "UserId1");

            migrationBuilder.AddForeignKey(
                name: "FK_AssessmentResults_AspNetUsers_UserId1",
                table: "AssessmentResults",
                column: "UserId1",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }
    }
}
