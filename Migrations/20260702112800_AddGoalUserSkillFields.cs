using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Career_Guidance_Platform.Migrations
{
    /// <inheritdoc />
    public partial class AddGoalUserSkillFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "cooldown_until",
                table: "user_skills",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "start_timestamp",
                table: "user_skills",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "difficulty",
                table: "skills",
                type: "varchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<int>(
                name: "estimated_hours",
                table: "skills",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "cooldown_until",
                table: "user_skills");

            migrationBuilder.DropColumn(
                name: "start_timestamp",
                table: "user_skills");

            migrationBuilder.DropColumn(
                name: "difficulty",
                table: "skills");

            migrationBuilder.DropColumn(
                name: "estimated_hours",
                table: "skills");
        }
    }
}
