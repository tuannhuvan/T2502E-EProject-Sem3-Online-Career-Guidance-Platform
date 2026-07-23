using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Career_Guidance_Platform.Migrations
{
    /// <inheritdoc />
    public partial class AddDateTakenToTestResult : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_test_answers_question_options_option_id",
                table: "test_answers");

            migrationBuilder.DropForeignKey(
                name: "FK_test_answers_test_results_result_id",
                table: "test_answers");

            migrationBuilder.DropForeignKey(
                name: "FK_test_results_AspNetUsers_student_id",
                table: "test_results");

            migrationBuilder.DropIndex(
                name: "IX_test_results_student_id",
                table: "test_results");

            migrationBuilder.DropColumn(
                name: "student_id",
                table: "test_results");

            migrationBuilder.RenameColumn(
                name: "result_id",
                table: "test_answers",
                newName: "test_result_id");

            migrationBuilder.RenameColumn(
                name: "option_id",
                table: "test_answers",
                newName: "question_option_id");

            migrationBuilder.RenameIndex(
                name: "IX_test_answers_result_id",
                table: "test_answers",
                newName: "IX_test_answers_test_result_id");

            migrationBuilder.RenameIndex(
                name: "IX_test_answers_option_id",
                table: "test_answers",
                newName: "IX_test_answers_question_option_id");

            migrationBuilder.AddColumn<DateTime>(
                name: "date_taken",
                table: "test_results",
                type: "datetime(6)",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "user_id",
                table: "test_results",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_test_results_user_id",
                table: "test_results",
                column: "user_id");

            migrationBuilder.AddForeignKey(
                name: "FK_test_answers_question_options_question_option_id",
                table: "test_answers",
                column: "question_option_id",
                principalTable: "question_options",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_test_answers_test_results_test_result_id",
                table: "test_answers",
                column: "test_result_id",
                principalTable: "test_results",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_test_results_AspNetUsers_user_id",
                table: "test_results",
                column: "user_id",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_test_answers_question_options_question_option_id",
                table: "test_answers");

            migrationBuilder.DropForeignKey(
                name: "FK_test_answers_test_results_test_result_id",
                table: "test_answers");

            migrationBuilder.DropForeignKey(
                name: "FK_test_results_AspNetUsers_user_id",
                table: "test_results");

            migrationBuilder.DropIndex(
                name: "IX_test_results_user_id",
                table: "test_results");

            migrationBuilder.DropColumn(
                name: "date_taken",
                table: "test_results");

            migrationBuilder.DropColumn(
                name: "user_id",
                table: "test_results");

            migrationBuilder.RenameColumn(
                name: "test_result_id",
                table: "test_answers",
                newName: "result_id");

            migrationBuilder.RenameColumn(
                name: "question_option_id",
                table: "test_answers",
                newName: "option_id");

            migrationBuilder.RenameIndex(
                name: "IX_test_answers_test_result_id",
                table: "test_answers",
                newName: "IX_test_answers_result_id");

            migrationBuilder.RenameIndex(
                name: "IX_test_answers_question_option_id",
                table: "test_answers",
                newName: "IX_test_answers_option_id");

            migrationBuilder.AddColumn<int>(
                name: "student_id",
                table: "test_results",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_test_results_student_id",
                table: "test_results",
                column: "student_id");

            migrationBuilder.AddForeignKey(
                name: "FK_test_answers_question_options_option_id",
                table: "test_answers",
                column: "option_id",
                principalTable: "question_options",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_test_answers_test_results_result_id",
                table: "test_answers",
                column: "result_id",
                principalTable: "test_results",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_test_results_AspNetUsers_student_id",
                table: "test_results",
                column: "student_id",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
