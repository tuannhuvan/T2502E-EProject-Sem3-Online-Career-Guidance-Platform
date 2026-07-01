using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Career_Guidance_Platform.Migrations
{
    /// <inheritdoc />
    public partial class UpdateSchemaForDetailedAspects : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_resources_skills_skill_id",
                table: "resources");

            migrationBuilder.DropTable(
                name: "goal_steps");

            migrationBuilder.DropColumn(
                name: "test_type",
                table: "tests");

            migrationBuilder.AddColumn<string>(
                name: "skill_type",
                table: "skills",
                type: "varchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<int>(
                name: "category_id",
                table: "resources",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "description",
                table: "resources",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<int>(
                name: "parent_resource_id",
                table: "resources",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "skill_id",
                table: "question_tests",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "test_type",
                table: "question_tests",
                type: "varchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "career_stages",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    career_path_id = table.Column<int>(type: "int", nullable: false),
                    title = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    description = table.Column<string>(type: "text", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    sequence_order = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_career_stages", x => x.id);
                    table.ForeignKey(
                        name: "FK_career_stages_career_paths_career_path_id",
                        column: x => x.career_path_id,
                        principalTable: "career_paths",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "goal_milestones",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    goal_id = table.Column<int>(type: "int", nullable: false),
                    title = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    status = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    sequence_order = table.Column<int>(type: "int", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    skill_id = table.Column<int>(type: "int", nullable: true),
                    resource_id = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_goal_milestones", x => x.id);
                    table.ForeignKey(
                        name: "FK_goal_milestones_goals_goal_id",
                        column: x => x.goal_id,
                        principalTable: "goals",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_goal_milestones_resources_resource_id",
                        column: x => x.resource_id,
                        principalTable: "resources",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_goal_milestones_skills_skill_id",
                        column: x => x.skill_id,
                        principalTable: "skills",
                        principalColumn: "id");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "career_stage_skills",
                columns: table => new
                {
                    career_stage_id = table.Column<int>(type: "int", nullable: false),
                    skill_id = table.Column<int>(type: "int", nullable: false),
                    proficiency_required = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_career_stage_skills", x => new { x.career_stage_id, x.skill_id });
                    table.ForeignKey(
                        name: "FK_career_stage_skills_career_stages_career_stage_id",
                        column: x => x.career_stage_id,
                        principalTable: "career_stages",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_career_stage_skills_skills_skill_id",
                        column: x => x.skill_id,
                        principalTable: "skills",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_resources_category_id",
                table: "resources",
                column: "category_id");

            migrationBuilder.CreateIndex(
                name: "IX_resources_parent_resource_id",
                table: "resources",
                column: "parent_resource_id");

            migrationBuilder.CreateIndex(
                name: "IX_question_tests_skill_id",
                table: "question_tests",
                column: "skill_id");

            migrationBuilder.CreateIndex(
                name: "IX_career_stage_skills_skill_id",
                table: "career_stage_skills",
                column: "skill_id");

            migrationBuilder.CreateIndex(
                name: "IX_career_stages_career_path_id",
                table: "career_stages",
                column: "career_path_id");

            migrationBuilder.CreateIndex(
                name: "IX_goal_milestones_goal_id",
                table: "goal_milestones",
                column: "goal_id");

            migrationBuilder.CreateIndex(
                name: "IX_goal_milestones_resource_id",
                table: "goal_milestones",
                column: "resource_id");

            migrationBuilder.CreateIndex(
                name: "IX_goal_milestones_skill_id",
                table: "goal_milestones",
                column: "skill_id");

            migrationBuilder.AddForeignKey(
                name: "FK_question_tests_skills_skill_id",
                table: "question_tests",
                column: "skill_id",
                principalTable: "skills",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_resources_categories_category_id",
                table: "resources",
                column: "category_id",
                principalTable: "categories",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_resources_resources_parent_resource_id",
                table: "resources",
                column: "parent_resource_id",
                principalTable: "resources",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_resources_skills_skill_id",
                table: "resources",
                column: "skill_id",
                principalTable: "skills",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_question_tests_skills_skill_id",
                table: "question_tests");

            migrationBuilder.DropForeignKey(
                name: "FK_resources_categories_category_id",
                table: "resources");

            migrationBuilder.DropForeignKey(
                name: "FK_resources_resources_parent_resource_id",
                table: "resources");

            migrationBuilder.DropForeignKey(
                name: "FK_resources_skills_skill_id",
                table: "resources");

            migrationBuilder.DropTable(
                name: "career_stage_skills");

            migrationBuilder.DropTable(
                name: "goal_milestones");

            migrationBuilder.DropTable(
                name: "career_stages");

            migrationBuilder.DropIndex(
                name: "IX_resources_category_id",
                table: "resources");

            migrationBuilder.DropIndex(
                name: "IX_resources_parent_resource_id",
                table: "resources");

            migrationBuilder.DropIndex(
                name: "IX_question_tests_skill_id",
                table: "question_tests");

            migrationBuilder.DropColumn(
                name: "skill_type",
                table: "skills");

            migrationBuilder.DropColumn(
                name: "category_id",
                table: "resources");

            migrationBuilder.DropColumn(
                name: "description",
                table: "resources");

            migrationBuilder.DropColumn(
                name: "parent_resource_id",
                table: "resources");

            migrationBuilder.DropColumn(
                name: "skill_id",
                table: "question_tests");

            migrationBuilder.DropColumn(
                name: "test_type",
                table: "question_tests");

            migrationBuilder.AddColumn<string>(
                name: "test_type",
                table: "tests",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "goal_steps",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    goal_id = table.Column<int>(type: "int", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    due_date = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    is_completed = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    sequence_order = table.Column<int>(type: "int", nullable: false),
                    title = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_goal_steps", x => x.id);
                    table.ForeignKey(
                        name: "FK_goal_steps_goals_goal_id",
                        column: x => x.goal_id,
                        principalTable: "goals",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_goal_steps_goal_id",
                table: "goal_steps",
                column: "goal_id");

            migrationBuilder.AddForeignKey(
                name: "FK_resources_skills_skill_id",
                table: "resources",
                column: "skill_id",
                principalTable: "skills",
                principalColumn: "id");
        }
    }
}
