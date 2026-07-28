using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Career_Guidance_Platform.Migrations
{
    /// <inheritdoc />
    public partial class userAnswerupdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserAnswers_QuestionOptions_QuestionOptionId",
                table: "UserAnswers");

            migrationBuilder.DropIndex(
                name: "IX_UserAnswers_QuestionOptionId",
                table: "UserAnswers");

            migrationBuilder.DropColumn(
                name: "QuestionOptionId",
                table: "UserAnswers");

            migrationBuilder.CreateIndex(
                name: "IX_UserAnswers_OptionId",
                table: "UserAnswers",
                column: "OptionId");

            migrationBuilder.AddForeignKey(
                name: "FK_UserAnswers_QuestionOptions_OptionId",
                table: "UserAnswers",
                column: "OptionId",
                principalTable: "QuestionOptions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserAnswers_QuestionOptions_OptionId",
                table: "UserAnswers");

            migrationBuilder.DropIndex(
                name: "IX_UserAnswers_OptionId",
                table: "UserAnswers");

            migrationBuilder.AddColumn<int>(
                name: "QuestionOptionId",
                table: "UserAnswers",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_UserAnswers_QuestionOptionId",
                table: "UserAnswers",
                column: "QuestionOptionId");

            migrationBuilder.AddForeignKey(
                name: "FK_UserAnswers_QuestionOptions_QuestionOptionId",
                table: "UserAnswers",
                column: "QuestionOptionId",
                principalTable: "QuestionOptions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
