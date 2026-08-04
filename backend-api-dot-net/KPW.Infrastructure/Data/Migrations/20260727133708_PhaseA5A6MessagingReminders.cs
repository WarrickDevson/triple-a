using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KPW.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class PhaseA5A6MessagingReminders : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ExerciseSessionLogs",
                columns: table => new
                {
                    ExerciseSessionLogId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PetId = table.Column<int>(type: "int", nullable: false),
                    ExerciseId = table.Column<int>(type: "int", nullable: false),
                    RehabProgramId = table.Column<int>(type: "int", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedUserId = table.Column<int>(type: "int", nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedUserId = table.Column<int>(type: "int", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExerciseSessionLogs", x => x.ExerciseSessionLogId);
                    table.ForeignKey(
                        name: "FK_ExerciseSessionLogs_Exercises_ExerciseId",
                        column: x => x.ExerciseId,
                        principalTable: "Exercises",
                        principalColumn: "ExerciseId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ExerciseSessionLogs_Pets_PetId",
                        column: x => x.PetId,
                        principalTable: "Pets",
                        principalColumn: "PetId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ExerciseSessionLogs_RehabPrograms_RehabProgramId",
                        column: x => x.RehabProgramId,
                        principalTable: "RehabPrograms",
                        principalColumn: "RehabProgramId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MessageThreads",
                columns: table => new
                {
                    MessageThreadId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PetId = table.Column<int>(type: "int", nullable: false),
                    OwnerId = table.Column<int>(type: "int", nullable: false),
                    PhysioId = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedUserId = table.Column<int>(type: "int", nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedUserId = table.Column<int>(type: "int", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MessageThreads", x => x.MessageThreadId);
                    table.ForeignKey(
                        name: "FK_MessageThreads_Pets_PetId",
                        column: x => x.PetId,
                        principalTable: "Pets",
                        principalColumn: "PetId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MessageThreads_Users_OwnerId",
                        column: x => x.OwnerId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MessageThreads_Users_PhysioId",
                        column: x => x.PhysioId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Messages",
                columns: table => new
                {
                    MessageId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MessageThreadId = table.Column<int>(type: "int", nullable: false),
                    SenderUserId = table.Column<int>(type: "int", nullable: false),
                    Body = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    VideoSubmissionId = table.Column<int>(type: "int", nullable: true),
                    ReadAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedUserId = table.Column<int>(type: "int", nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedUserId = table.Column<int>(type: "int", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Messages", x => x.MessageId);
                    table.ForeignKey(
                        name: "FK_Messages_MessageThreads_MessageThreadId",
                        column: x => x.MessageThreadId,
                        principalTable: "MessageThreads",
                        principalColumn: "MessageThreadId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Messages_Users_SenderUserId",
                        column: x => x.SenderUserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Messages_VideoSubmissions_VideoSubmissionId",
                        column: x => x.VideoSubmissionId,
                        principalTable: "VideoSubmissions",
                        principalColumn: "VideoSubmissionId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ExerciseSessionLogs_ExerciseId",
                table: "ExerciseSessionLogs",
                column: "ExerciseId");

            migrationBuilder.CreateIndex(
                name: "IX_ExerciseSessionLogs_PetId_ExerciseId_CompletedAt",
                table: "ExerciseSessionLogs",
                columns: new[] { "PetId", "ExerciseId", "CompletedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_ExerciseSessionLogs_RehabProgramId",
                table: "ExerciseSessionLogs",
                column: "RehabProgramId");

            migrationBuilder.CreateIndex(
                name: "IX_Messages_MessageThreadId",
                table: "Messages",
                column: "MessageThreadId");

            migrationBuilder.CreateIndex(
                name: "IX_Messages_SenderUserId",
                table: "Messages",
                column: "SenderUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Messages_VideoSubmissionId",
                table: "Messages",
                column: "VideoSubmissionId");

            migrationBuilder.CreateIndex(
                name: "IX_MessageThreads_OwnerId",
                table: "MessageThreads",
                column: "OwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_MessageThreads_PetId",
                table: "MessageThreads",
                column: "PetId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MessageThreads_PhysioId",
                table: "MessageThreads",
                column: "PhysioId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ExerciseSessionLogs");

            migrationBuilder.DropTable(
                name: "Messages");

            migrationBuilder.DropTable(
                name: "MessageThreads");
        }
    }
}
