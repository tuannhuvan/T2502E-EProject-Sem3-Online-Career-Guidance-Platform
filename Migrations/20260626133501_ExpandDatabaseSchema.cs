using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Career_Guidance_Platform.Migrations
{
    /// <inheritdoc />
    public partial class ExpandDatabaseSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_goals_career_paths_career_path_id",
                table: "goals");

            migrationBuilder.AddColumn<string>(
                name: "test_type",
                table: "tests",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<int>(
                name: "template_id",
                table: "resumes",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "skill_id",
                table: "resources",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "application_url",
                table: "job_postings",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "experience_level",
                table: "job_postings",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<DateTime>(
                name: "expired_at",
                table: "job_postings",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "job_type",
                table: "job_postings",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "location",
                table: "job_postings",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "goal_type",
                table: "goals",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<int>(
                name: "author_id",
                table: "community_posts",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "job_outlook",
                table: "career_paths",
                type: "double",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<int>(
                name: "parent_path_id",
                table: "career_paths",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "salary_max",
                table: "career_paths",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "salary_min",
                table: "career_paths",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateTable(
                name: "community_comments",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    post_id = table.Column<int>(type: "int", nullable: false),
                    author_id = table.Column<int>(type: "int", nullable: false),
                    content = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    created_at = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_community_comments", x => x.id);
                    table.ForeignKey(
                        name: "FK_community_comments_AspNetUsers_author_id",
                        column: x => x.author_id,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_community_comments_community_posts_post_id",
                        column: x => x.post_id,
                        principalTable: "community_posts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "employer_reviews",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    user_id = table.Column<int>(type: "int", nullable: false),
                    company_name = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    job_title = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    overall_rating = table.Column<int>(type: "int", nullable: false),
                    culture_rating = table.Column<int>(type: "int", nullable: false),
                    work_life_balance_rating = table.Column<int>(type: "int", nullable: false),
                    career_growth_rating = table.Column<int>(type: "int", nullable: false),
                    review_content = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    pros = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    cons = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    created_at = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_employer_reviews", x => x.id);
                    table.ForeignKey(
                        name: "FK_employer_reviews_AspNetUsers_user_id",
                        column: x => x.user_id,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "event_registrations",
                columns: table => new
                {
                    event_id = table.Column<int>(type: "int", nullable: false),
                    user_id = table.Column<int>(type: "int", nullable: false),
                    registered_at = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_event_registrations", x => new { x.event_id, x.user_id });
                    table.ForeignKey(
                        name: "FK_event_registrations_AspNetUsers_user_id",
                        column: x => x.user_id,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_event_registrations_career_events_event_id",
                        column: x => x.event_id,
                        principalTable: "career_events",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "goal_steps",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    goal_id = table.Column<int>(type: "int", nullable: false),
                    title = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    is_completed = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    sequence_order = table.Column<int>(type: "int", nullable: false),
                    due_date = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime(6)", nullable: false)
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

            migrationBuilder.CreateTable(
                name: "job_applications",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    user_id = table.Column<int>(type: "int", nullable: false),
                    job_posting_id = table.Column<int>(type: "int", nullable: false),
                    resume_id = table.Column<int>(type: "int", nullable: true),
                    status = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    applied_at = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    notes = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_job_applications", x => x.id);
                    table.ForeignKey(
                        name: "FK_job_applications_AspNetUsers_user_id",
                        column: x => x.user_id,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_job_applications_job_postings_job_posting_id",
                        column: x => x.job_posting_id,
                        principalTable: "job_postings",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_job_applications_resumes_resume_id",
                        column: x => x.resume_id,
                        principalTable: "resumes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "mentor_profiles",
                columns: table => new
                {
                    user_id = table.Column<int>(type: "int", nullable: false),
                    job_title = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    company = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    specialization = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    biography = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    availability_json = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    linkedin_url = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    rating = table.Column<decimal>(type: "decimal(3,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_mentor_profiles", x => x.user_id);
                    table.ForeignKey(
                        name: "FK_mentor_profiles_AspNetUsers_user_id",
                        column: x => x.user_id,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "mentorship_messages",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    sender_id = table.Column<int>(type: "int", nullable: false),
                    receiver_id = table.Column<int>(type: "int", nullable: false),
                    content = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    created_at = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    is_read = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_mentorship_messages", x => x.id);
                    table.ForeignKey(
                        name: "FK_mentorship_messages_AspNetUsers_receiver_id",
                        column: x => x.receiver_id,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_mentorship_messages_AspNetUsers_sender_id",
                        column: x => x.sender_id,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "peer_connections",
                columns: table => new
                {
                    requester_id = table.Column<int>(type: "int", nullable: false),
                    receiver_id = table.Column<int>(type: "int", nullable: false),
                    status = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    sent_at = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    connected_at = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_peer_connections", x => new { x.requester_id, x.receiver_id });
                    table.ForeignKey(
                        name: "FK_peer_connections_AspNetUsers_receiver_id",
                        column: x => x.receiver_id,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_peer_connections_AspNetUsers_requester_id",
                        column: x => x.requester_id,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "resume_templates",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    name = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    thumbnail_url = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    template_code = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    is_active = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_resume_templates", x => x.id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "saved_jobs",
                columns: table => new
                {
                    user_id = table.Column<int>(type: "int", nullable: false),
                    job_posting_id = table.Column<int>(type: "int", nullable: false),
                    saved_at = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_saved_jobs", x => new { x.user_id, x.job_posting_id });
                    table.ForeignKey(
                        name: "FK_saved_jobs_AspNetUsers_user_id",
                        column: x => x.user_id,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_saved_jobs_job_postings_job_posting_id",
                        column: x => x.job_posting_id,
                        principalTable: "job_postings",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "skills",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    name = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    description = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    created_at = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    created_by = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    updated_at = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    updated_by = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    status = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_skills", x => x.id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "success_stories",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    career_path_id = table.Column<int>(type: "int", nullable: false),
                    professional_name = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    job_title = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    company_name = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    story_content = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    image_url = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    linkedin_url = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    created_at = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_success_stories", x => x.id);
                    table.ForeignKey(
                        name: "FK_success_stories_career_paths_career_path_id",
                        column: x => x.career_path_id,
                        principalTable: "career_paths",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "test_result_scores",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    test_result_id = table.Column<int>(type: "int", nullable: false),
                    career_path_id = table.Column<int>(type: "int", nullable: false),
                    score = table.Column<decimal>(type: "decimal(5,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_test_result_scores", x => x.id);
                    table.ForeignKey(
                        name: "FK_test_result_scores_career_paths_career_path_id",
                        column: x => x.career_path_id,
                        principalTable: "career_paths",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_test_result_scores_test_results_test_result_id",
                        column: x => x.test_result_id,
                        principalTable: "test_results",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "application_reminders",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    user_id = table.Column<int>(type: "int", nullable: false),
                    job_application_id = table.Column<int>(type: "int", nullable: true),
                    reminder_date = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    message = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    is_completed = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_application_reminders", x => x.id);
                    table.ForeignKey(
                        name: "FK_application_reminders_AspNetUsers_user_id",
                        column: x => x.user_id,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_application_reminders_job_applications_job_application_id",
                        column: x => x.job_application_id,
                        principalTable: "job_applications",
                        principalColumn: "id");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "group_mentoring_sessions",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    mentor_id = table.Column<int>(type: "int", nullable: false),
                    title = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    description = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    scheduled_time = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    meeting_url = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    max_participants = table.Column<int>(type: "int", nullable: false),
                    status = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    created_at = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    MentorProfileUserId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_group_mentoring_sessions", x => x.id);
                    table.ForeignKey(
                        name: "FK_group_mentoring_sessions_AspNetUsers_mentor_id",
                        column: x => x.mentor_id,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_group_mentoring_sessions_mentor_profiles_MentorProfileUserId",
                        column: x => x.MentorProfileUserId,
                        principalTable: "mentor_profiles",
                        principalColumn: "user_id");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "mentorship_meetings",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    mentee_id = table.Column<int>(type: "int", nullable: false),
                    mentor_id = table.Column<int>(type: "int", nullable: false),
                    title = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    description = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    scheduled_time = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    meeting_url = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    status = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    created_at = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    MentorProfileUserId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_mentorship_meetings", x => x.id);
                    table.ForeignKey(
                        name: "FK_mentorship_meetings_AspNetUsers_mentee_id",
                        column: x => x.mentee_id,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_mentorship_meetings_AspNetUsers_mentor_id",
                        column: x => x.mentor_id,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_mentorship_meetings_mentor_profiles_MentorProfileUserId",
                        column: x => x.MentorProfileUserId,
                        principalTable: "mentor_profiles",
                        principalColumn: "user_id");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "mentorship_requests",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    mentee_id = table.Column<int>(type: "int", nullable: false),
                    mentor_id = table.Column<int>(type: "int", nullable: false),
                    message = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    status = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    created_at = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    MentorProfileUserId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_mentorship_requests", x => x.id);
                    table.ForeignKey(
                        name: "FK_mentorship_requests_AspNetUsers_mentee_id",
                        column: x => x.mentee_id,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_mentorship_requests_AspNetUsers_mentor_id",
                        column: x => x.mentor_id,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_mentorship_requests_mentor_profiles_MentorProfileUserId",
                        column: x => x.MentorProfileUserId,
                        principalTable: "mentor_profiles",
                        principalColumn: "user_id");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "career_path_skills",
                columns: table => new
                {
                    career_path_id = table.Column<int>(type: "int", nullable: false),
                    skill_id = table.Column<int>(type: "int", nullable: false),
                    importance_level = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_career_path_skills", x => new { x.career_path_id, x.skill_id });
                    table.ForeignKey(
                        name: "FK_career_path_skills_career_paths_career_path_id",
                        column: x => x.career_path_id,
                        principalTable: "career_paths",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_career_path_skills_skills_skill_id",
                        column: x => x.skill_id,
                        principalTable: "skills",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "user_skills",
                columns: table => new
                {
                    user_id = table.Column<int>(type: "int", nullable: false),
                    skill_id = table.Column<int>(type: "int", nullable: false),
                    proficiency_level = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    status = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_skills", x => new { x.user_id, x.skill_id });
                    table.ForeignKey(
                        name: "FK_user_skills_AspNetUsers_user_id",
                        column: x => x.user_id,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_user_skills_skills_skill_id",
                        column: x => x.skill_id,
                        principalTable: "skills",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "group_mentoring_registrations",
                columns: table => new
                {
                    session_id = table.Column<int>(type: "int", nullable: false),
                    student_id = table.Column<int>(type: "int", nullable: false),
                    registered_at = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_group_mentoring_registrations", x => new { x.session_id, x.student_id });
                    table.ForeignKey(
                        name: "FK_group_mentoring_registrations_AspNetUsers_student_id",
                        column: x => x.student_id,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_group_mentoring_registrations_group_mentoring_sessions_sessi~",
                        column: x => x.session_id,
                        principalTable: "group_mentoring_sessions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_resumes_template_id",
                table: "resumes",
                column: "template_id");

            migrationBuilder.CreateIndex(
                name: "IX_resources_skill_id",
                table: "resources",
                column: "skill_id");

            migrationBuilder.CreateIndex(
                name: "IX_community_posts_author_id",
                table: "community_posts",
                column: "author_id");

            migrationBuilder.CreateIndex(
                name: "IX_career_paths_parent_path_id",
                table: "career_paths",
                column: "parent_path_id");

            migrationBuilder.CreateIndex(
                name: "IX_application_reminders_job_application_id",
                table: "application_reminders",
                column: "job_application_id");

            migrationBuilder.CreateIndex(
                name: "IX_application_reminders_user_id",
                table: "application_reminders",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_career_path_skills_skill_id",
                table: "career_path_skills",
                column: "skill_id");

            migrationBuilder.CreateIndex(
                name: "IX_community_comments_author_id",
                table: "community_comments",
                column: "author_id");

            migrationBuilder.CreateIndex(
                name: "IX_community_comments_post_id",
                table: "community_comments",
                column: "post_id");

            migrationBuilder.CreateIndex(
                name: "IX_employer_reviews_user_id",
                table: "employer_reviews",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_event_registrations_user_id",
                table: "event_registrations",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_goal_steps_goal_id",
                table: "goal_steps",
                column: "goal_id");

            migrationBuilder.CreateIndex(
                name: "IX_group_mentoring_registrations_student_id",
                table: "group_mentoring_registrations",
                column: "student_id");

            migrationBuilder.CreateIndex(
                name: "IX_group_mentoring_sessions_mentor_id",
                table: "group_mentoring_sessions",
                column: "mentor_id");

            migrationBuilder.CreateIndex(
                name: "IX_group_mentoring_sessions_MentorProfileUserId",
                table: "group_mentoring_sessions",
                column: "MentorProfileUserId");

            migrationBuilder.CreateIndex(
                name: "IX_job_applications_job_posting_id",
                table: "job_applications",
                column: "job_posting_id");

            migrationBuilder.CreateIndex(
                name: "IX_job_applications_resume_id",
                table: "job_applications",
                column: "resume_id");

            migrationBuilder.CreateIndex(
                name: "IX_job_applications_user_id",
                table: "job_applications",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_mentorship_meetings_mentee_id",
                table: "mentorship_meetings",
                column: "mentee_id");

            migrationBuilder.CreateIndex(
                name: "IX_mentorship_meetings_mentor_id",
                table: "mentorship_meetings",
                column: "mentor_id");

            migrationBuilder.CreateIndex(
                name: "IX_mentorship_meetings_MentorProfileUserId",
                table: "mentorship_meetings",
                column: "MentorProfileUserId");

            migrationBuilder.CreateIndex(
                name: "IX_mentorship_messages_receiver_id",
                table: "mentorship_messages",
                column: "receiver_id");

            migrationBuilder.CreateIndex(
                name: "IX_mentorship_messages_sender_id",
                table: "mentorship_messages",
                column: "sender_id");

            migrationBuilder.CreateIndex(
                name: "IX_mentorship_requests_mentee_id",
                table: "mentorship_requests",
                column: "mentee_id");

            migrationBuilder.CreateIndex(
                name: "IX_mentorship_requests_mentor_id",
                table: "mentorship_requests",
                column: "mentor_id");

            migrationBuilder.CreateIndex(
                name: "IX_mentorship_requests_MentorProfileUserId",
                table: "mentorship_requests",
                column: "MentorProfileUserId");

            migrationBuilder.CreateIndex(
                name: "IX_peer_connections_receiver_id",
                table: "peer_connections",
                column: "receiver_id");

            migrationBuilder.CreateIndex(
                name: "IX_saved_jobs_job_posting_id",
                table: "saved_jobs",
                column: "job_posting_id");

            migrationBuilder.CreateIndex(
                name: "IX_success_stories_career_path_id",
                table: "success_stories",
                column: "career_path_id");

            migrationBuilder.CreateIndex(
                name: "IX_test_result_scores_career_path_id",
                table: "test_result_scores",
                column: "career_path_id");

            migrationBuilder.CreateIndex(
                name: "IX_test_result_scores_test_result_id",
                table: "test_result_scores",
                column: "test_result_id");

            migrationBuilder.CreateIndex(
                name: "IX_user_skills_skill_id",
                table: "user_skills",
                column: "skill_id");

            migrationBuilder.AddForeignKey(
                name: "FK_career_paths_career_paths_parent_path_id",
                table: "career_paths",
                column: "parent_path_id",
                principalTable: "career_paths",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_community_posts_AspNetUsers_author_id",
                table: "community_posts",
                column: "author_id",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_goals_career_paths_career_path_id",
                table: "goals",
                column: "career_path_id",
                principalTable: "career_paths",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_resources_skills_skill_id",
                table: "resources",
                column: "skill_id",
                principalTable: "skills",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_resumes_resume_templates_template_id",
                table: "resumes",
                column: "template_id",
                principalTable: "resume_templates",
                principalColumn: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_career_paths_career_paths_parent_path_id",
                table: "career_paths");

            migrationBuilder.DropForeignKey(
                name: "FK_community_posts_AspNetUsers_author_id",
                table: "community_posts");

            migrationBuilder.DropForeignKey(
                name: "FK_goals_career_paths_career_path_id",
                table: "goals");

            migrationBuilder.DropForeignKey(
                name: "FK_resources_skills_skill_id",
                table: "resources");

            migrationBuilder.DropForeignKey(
                name: "FK_resumes_resume_templates_template_id",
                table: "resumes");

            migrationBuilder.DropTable(
                name: "application_reminders");

            migrationBuilder.DropTable(
                name: "career_path_skills");

            migrationBuilder.DropTable(
                name: "community_comments");

            migrationBuilder.DropTable(
                name: "employer_reviews");

            migrationBuilder.DropTable(
                name: "event_registrations");

            migrationBuilder.DropTable(
                name: "goal_steps");

            migrationBuilder.DropTable(
                name: "group_mentoring_registrations");

            migrationBuilder.DropTable(
                name: "mentorship_meetings");

            migrationBuilder.DropTable(
                name: "mentorship_messages");

            migrationBuilder.DropTable(
                name: "mentorship_requests");

            migrationBuilder.DropTable(
                name: "peer_connections");

            migrationBuilder.DropTable(
                name: "resume_templates");

            migrationBuilder.DropTable(
                name: "saved_jobs");

            migrationBuilder.DropTable(
                name: "success_stories");

            migrationBuilder.DropTable(
                name: "test_result_scores");

            migrationBuilder.DropTable(
                name: "user_skills");

            migrationBuilder.DropTable(
                name: "job_applications");

            migrationBuilder.DropTable(
                name: "group_mentoring_sessions");

            migrationBuilder.DropTable(
                name: "skills");

            migrationBuilder.DropTable(
                name: "mentor_profiles");

            migrationBuilder.DropIndex(
                name: "IX_resumes_template_id",
                table: "resumes");

            migrationBuilder.DropIndex(
                name: "IX_resources_skill_id",
                table: "resources");

            migrationBuilder.DropIndex(
                name: "IX_community_posts_author_id",
                table: "community_posts");

            migrationBuilder.DropIndex(
                name: "IX_career_paths_parent_path_id",
                table: "career_paths");

            migrationBuilder.DropColumn(
                name: "test_type",
                table: "tests");

            migrationBuilder.DropColumn(
                name: "template_id",
                table: "resumes");

            migrationBuilder.DropColumn(
                name: "skill_id",
                table: "resources");

            migrationBuilder.DropColumn(
                name: "application_url",
                table: "job_postings");

            migrationBuilder.DropColumn(
                name: "experience_level",
                table: "job_postings");

            migrationBuilder.DropColumn(
                name: "expired_at",
                table: "job_postings");

            migrationBuilder.DropColumn(
                name: "job_type",
                table: "job_postings");

            migrationBuilder.DropColumn(
                name: "location",
                table: "job_postings");

            migrationBuilder.DropColumn(
                name: "goal_type",
                table: "goals");

            migrationBuilder.DropColumn(
                name: "author_id",
                table: "community_posts");

            migrationBuilder.DropColumn(
                name: "job_outlook",
                table: "career_paths");

            migrationBuilder.DropColumn(
                name: "parent_path_id",
                table: "career_paths");

            migrationBuilder.DropColumn(
                name: "salary_max",
                table: "career_paths");

            migrationBuilder.DropColumn(
                name: "salary_min",
                table: "career_paths");

            migrationBuilder.AddForeignKey(
                name: "FK_goals_career_paths_career_path_id",
                table: "goals",
                column: "career_path_id",
                principalTable: "career_paths",
                principalColumn: "id");
        }
    }
}
