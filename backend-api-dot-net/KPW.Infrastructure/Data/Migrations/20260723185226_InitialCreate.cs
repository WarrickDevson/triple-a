using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace KPW.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Clinics",
                columns: table => new
                {
                    ClinicId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ClinicName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    VatNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    PhysicalAddress = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    ContactNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedUserId = table.Column<int>(type: "int", nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedUserId = table.Column<int>(type: "int", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Clinics", x => x.ClinicId);
                });

            migrationBuilder.CreateTable(
                name: "Exercises",
                columns: table => new
                {
                    ExerciseId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    ShortDescription = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    TargetedMuscles = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    ClinicalPurpose = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    SafetyNotes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CommonMistakes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    VideoUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    DifficultyLevel = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedUserId = table.Column<int>(type: "int", nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedUserId = table.Column<int>(type: "int", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Exercises", x => x.ExerciseId);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ClinicId = table.Column<int>(type: "int", nullable: true),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    FirstName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    LastName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    PhoneNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    UserRole = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    SubscriptionTier = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    RefreshTokenHash = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    RefreshTokenExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedUserId = table.Column<int>(type: "int", nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedUserId = table.Column<int>(type: "int", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.UserId);
                    table.ForeignKey(
                        name: "FK_Users_Clinics_ClinicId",
                        column: x => x.ClinicId,
                        principalTable: "Clinics",
                        principalColumn: "ClinicId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ExerciseSteps",
                columns: table => new
                {
                    ExerciseStepId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ExerciseId = table.Column<int>(type: "int", nullable: false),
                    StepNumber = table.Column<int>(type: "int", nullable: false),
                    StepInstruction = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    ImageUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedUserId = table.Column<int>(type: "int", nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedUserId = table.Column<int>(type: "int", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExerciseSteps", x => x.ExerciseStepId);
                    table.ForeignKey(
                        name: "FK_ExerciseSteps_Exercises_ExerciseId",
                        column: x => x.ExerciseId,
                        principalTable: "Exercises",
                        principalColumn: "ExerciseId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Pets",
                columns: table => new
                {
                    PetId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OwnerId = table.Column<int>(type: "int", nullable: false),
                    PetName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Species = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Breed = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    BirthDate = table.Column<DateOnly>(type: "date", nullable: true),
                    WeightKg = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedUserId = table.Column<int>(type: "int", nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedUserId = table.Column<int>(type: "int", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Pets", x => x.PetId);
                    table.ForeignKey(
                        name: "FK_Pets_Users_OwnerId",
                        column: x => x.OwnerId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Appointments",
                columns: table => new
                {
                    AppointmentId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PhysioId = table.Column<int>(type: "int", nullable: false),
                    OwnerId = table.Column<int>(type: "int", nullable: false),
                    PetId = table.Column<int>(type: "int", nullable: false),
                    ScheduledDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AppointmentStatus = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ClientNotes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ClinicianNotes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedUserId = table.Column<int>(type: "int", nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedUserId = table.Column<int>(type: "int", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Appointments", x => x.AppointmentId);
                    table.ForeignKey(
                        name: "FK_Appointments_Pets_PetId",
                        column: x => x.PetId,
                        principalTable: "Pets",
                        principalColumn: "PetId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Appointments_Users_OwnerId",
                        column: x => x.OwnerId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Appointments_Users_PhysioId",
                        column: x => x.PhysioId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DailyTrackingLogs",
                columns: table => new
                {
                    LogId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PetId = table.Column<int>(type: "int", nullable: false),
                    LogDate = table.Column<DateOnly>(type: "date", nullable: false),
                    PainScore = table.Column<int>(type: "int", nullable: true),
                    LamenessScore = table.Column<int>(type: "int", nullable: true),
                    EnergyScore = table.Column<int>(type: "int", nullable: true),
                    AppetiteScore = table.Column<int>(type: "int", nullable: true),
                    MobilityScore = table.Column<int>(type: "int", nullable: true),
                    WeightKg = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: true),
                    IsCompleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedUserId = table.Column<int>(type: "int", nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedUserId = table.Column<int>(type: "int", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DailyTrackingLogs", x => x.LogId);
                    table.ForeignKey(
                        name: "FK_DailyTrackingLogs_Pets_PetId",
                        column: x => x.PetId,
                        principalTable: "Pets",
                        principalColumn: "PetId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MedicalHistories",
                columns: table => new
                {
                    MedicalHistoryId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PetId = table.Column<int>(type: "int", nullable: false),
                    Diagnosis = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    InjuryOrCondition = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SurgeryDate = table.Column<DateOnly>(type: "date", nullable: true),
                    ClinicianNotes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedUserId = table.Column<int>(type: "int", nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedUserId = table.Column<int>(type: "int", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MedicalHistories", x => x.MedicalHistoryId);
                    table.ForeignKey(
                        name: "FK_MedicalHistories_Pets_PetId",
                        column: x => x.PetId,
                        principalTable: "Pets",
                        principalColumn: "PetId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RehabPrograms",
                columns: table => new
                {
                    RehabProgramId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PhysioId = table.Column<int>(type: "int", nullable: false),
                    PetId = table.Column<int>(type: "int", nullable: false),
                    ProgramTitle = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    StartDate = table.Column<DateOnly>(type: "date", nullable: false),
                    EndDate = table.Column<DateOnly>(type: "date", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedUserId = table.Column<int>(type: "int", nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedUserId = table.Column<int>(type: "int", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RehabPrograms", x => x.RehabProgramId);
                    table.ForeignKey(
                        name: "FK_RehabPrograms_Pets_PetId",
                        column: x => x.PetId,
                        principalTable: "Pets",
                        principalColumn: "PetId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RehabPrograms_Users_PhysioId",
                        column: x => x.PhysioId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "VideoSubmissions",
                columns: table => new
                {
                    VideoSubmissionId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PetId = table.Column<int>(type: "int", nullable: false),
                    ExerciseId = table.Column<int>(type: "int", nullable: false),
                    RawVideoStorageUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    ProcessedVideoStreamingUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    PhysioFeedbackNotes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsReviewed = table.Column<bool>(type: "bit", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedUserId = table.Column<int>(type: "int", nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedUserId = table.Column<int>(type: "int", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VideoSubmissions", x => x.VideoSubmissionId);
                    table.ForeignKey(
                        name: "FK_VideoSubmissions_Exercises_ExerciseId",
                        column: x => x.ExerciseId,
                        principalTable: "Exercises",
                        principalColumn: "ExerciseId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_VideoSubmissions_Pets_PetId",
                        column: x => x.PetId,
                        principalTable: "Pets",
                        principalColumn: "PetId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RehabProgramExercises",
                columns: table => new
                {
                    RehabProgramExerciseId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RehabProgramId = table.Column<int>(type: "int", nullable: false),
                    ExerciseId = table.Column<int>(type: "int", nullable: false),
                    Repetitions = table.Column<int>(type: "int", nullable: false),
                    Sets = table.Column<int>(type: "int", nullable: false),
                    FrequencyPerDay = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedUserId = table.Column<int>(type: "int", nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedUserId = table.Column<int>(type: "int", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RehabProgramExercises", x => x.RehabProgramExerciseId);
                    table.ForeignKey(
                        name: "FK_RehabProgramExercises_Exercises_ExerciseId",
                        column: x => x.ExerciseId,
                        principalTable: "Exercises",
                        principalColumn: "ExerciseId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RehabProgramExercises_RehabPrograms_RehabProgramId",
                        column: x => x.RehabProgramId,
                        principalTable: "RehabPrograms",
                        principalColumn: "RehabProgramId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "Clinics",
                columns: new[] { "ClinicId", "ClinicName", "ContactNumber", "CreatedDate", "CreatedUserId", "IsActive", "ModifiedDate", "ModifiedUserId", "PhysicalAddress", "VatNumber" },
                values: new object[] { 1, "Kruger's Pet Wellness - Demo Clinic", "+27110000000", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "123 Wellness Street, Pretoria, Gauteng", "4123456789" });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "UserId", "ClinicId", "CreatedDate", "CreatedUserId", "Email", "FirstName", "IsActive", "LastName", "ModifiedDate", "ModifiedUserId", "PasswordHash", "PhoneNumber", "RefreshTokenExpiresAt", "RefreshTokenHash", "SubscriptionTier", "UserRole" },
                values: new object[,]
                {
                    { 3, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "owner@kpw.local", "Demo", true, "Owner", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "$argon2id$v=19$m=65536,t=3,p=1$pE6t/ab4vs6xAn1qMByDZQ$GtKjB012JDKkMeTUDEMOBROTGRMOp2ubJZIZkE06NfI", "+27110000003", null, null, "Free", "Owner" },
                    { 1, 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "sysadmin@kpw.local", "System", true, "Administrator", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "$argon2id$v=19$m=65536,t=3,p=1$pE6t/ab4vs6xAn1qMByDZQ$GtKjB012JDKkMeTUDEMOBROTGRMOp2ubJZIZkE06NfI", "+27110000001", null, null, "Pro", "SysAdmin" },
                    { 2, 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "physio@kpw.local", "Demo", true, "Physiotherapist", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "$argon2id$v=19$m=65536,t=3,p=1$pE6t/ab4vs6xAn1qMByDZQ$GtKjB012JDKkMeTUDEMOBROTGRMOp2ubJZIZkE06NfI", "+27110000002", null, null, "Pro", "Physio" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Appointments_OwnerId",
                table: "Appointments",
                column: "OwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_Appointments_PetId",
                table: "Appointments",
                column: "PetId");

            migrationBuilder.CreateIndex(
                name: "IX_Appointments_PhysioId",
                table: "Appointments",
                column: "PhysioId");

            migrationBuilder.CreateIndex(
                name: "IX_DailyTrackingLogs_PetId",
                table: "DailyTrackingLogs",
                column: "PetId");

            migrationBuilder.CreateIndex(
                name: "IX_ExerciseSteps_ExerciseId",
                table: "ExerciseSteps",
                column: "ExerciseId");

            migrationBuilder.CreateIndex(
                name: "IX_MedicalHistories_PetId",
                table: "MedicalHistories",
                column: "PetId");

            migrationBuilder.CreateIndex(
                name: "IX_Pets_OwnerId",
                table: "Pets",
                column: "OwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_RehabProgramExercises_ExerciseId",
                table: "RehabProgramExercises",
                column: "ExerciseId");

            migrationBuilder.CreateIndex(
                name: "IX_RehabProgramExercises_RehabProgramId",
                table: "RehabProgramExercises",
                column: "RehabProgramId");

            migrationBuilder.CreateIndex(
                name: "IX_RehabPrograms_PetId",
                table: "RehabPrograms",
                column: "PetId");

            migrationBuilder.CreateIndex(
                name: "IX_RehabPrograms_PhysioId",
                table: "RehabPrograms",
                column: "PhysioId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_ClinicId",
                table: "Users",
                column: "ClinicId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_VideoSubmissions_ExerciseId",
                table: "VideoSubmissions",
                column: "ExerciseId");

            migrationBuilder.CreateIndex(
                name: "IX_VideoSubmissions_PetId",
                table: "VideoSubmissions",
                column: "PetId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Appointments");

            migrationBuilder.DropTable(
                name: "DailyTrackingLogs");

            migrationBuilder.DropTable(
                name: "ExerciseSteps");

            migrationBuilder.DropTable(
                name: "MedicalHistories");

            migrationBuilder.DropTable(
                name: "RehabProgramExercises");

            migrationBuilder.DropTable(
                name: "VideoSubmissions");

            migrationBuilder.DropTable(
                name: "RehabPrograms");

            migrationBuilder.DropTable(
                name: "Exercises");

            migrationBuilder.DropTable(
                name: "Pets");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "Clinics");
        }
    }
}
