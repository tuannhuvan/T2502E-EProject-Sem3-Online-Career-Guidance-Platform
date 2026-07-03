using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Career_Guidance_Platform.Migrations
{
    /// <inheritdoc />
    public partial class AddMentorshipUpdates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "experience_description",
                table: "mentor_profiles",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "expertise",
                table: "mentor_profiles",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<decimal>(
                name: "hourly_rate",
                table: "mentor_profiles",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<bool>(
                name: "is_active",
                table: "mentor_profiles",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "is_verified",
                table: "mentor_profiles",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "mentor_reviews",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    mentor_id = table.Column<int>(type: "int", nullable: false),
                    mentee_id = table.Column<int>(type: "int", nullable: false),
                    meeting_id = table.Column<int>(type: "int", nullable: false),
                    rating = table.Column<int>(type: "int", nullable: false),
                    comment = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    created_at = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_mentor_reviews", x => x.id);
                    table.ForeignKey(
                        name: "FK_mentor_reviews_AspNetUsers_mentee_id",
                        column: x => x.mentee_id,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_mentor_reviews_AspNetUsers_mentor_id",
                        column: x => x.mentor_id,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_mentor_reviews_mentorship_meetings_meeting_id",
                        column: x => x.meeting_id,
                        principalTable: "mentorship_meetings",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_mentor_reviews_meeting_id",
                table: "mentor_reviews",
                column: "meeting_id");

            migrationBuilder.CreateIndex(
                name: "IX_mentor_reviews_mentee_id",
                table: "mentor_reviews",
                column: "mentee_id");

            migrationBuilder.CreateIndex(
                name: "IX_mentor_reviews_mentor_id",
                table: "mentor_reviews",
                column: "mentor_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "mentor_reviews");

            migrationBuilder.DropColumn(
                name: "experience_description",
                table: "mentor_profiles");

            migrationBuilder.DropColumn(
                name: "expertise",
                table: "mentor_profiles");

            migrationBuilder.DropColumn(
                name: "hourly_rate",
                table: "mentor_profiles");

            migrationBuilder.DropColumn(
                name: "is_active",
                table: "mentor_profiles");

            migrationBuilder.DropColumn(
                name: "is_verified",
                table: "mentor_profiles");
        }
    }
}
