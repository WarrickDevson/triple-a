using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace KPW.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddExerciseFiltersAndDemoProgram : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ConditionCategory",
                table: "Exercises",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TargetSpecies",
                table: "Exercises",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.InsertData(
                table: "Exercises",
                columns: new[] { "ExerciseId", "ClinicalPurpose", "CommonMistakes", "ConditionCategory", "CreatedDate", "CreatedUserId", "DifficultyLevel", "IsActive", "ModifiedDate", "ModifiedUserId", "SafetyNotes", "ShortDescription", "TargetSpecies", "TargetedMuscles", "Title", "VideoUrl" },
                values: new object[,]
                {
                    { 1, "Improve weight-bearing tolerance after hip dysplasia diagnosis.", "Allowing the dog to collapse backward instead of pushing through the hind limbs.", "HipDysplasia", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 2, true, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Stop if the dog shows pain, vocalises, or refuses to stand.", "Build hind-limb strength through controlled transitions.", "Canine", "Gluteals, quadriceps, hamstrings", "Sit-to-Stand", "https://commondatastorage.googleapis.com/gtv-videos-bucket/sample/ForBiggerBlazes.mp4" },
                    { 2, "Maintain joint range before active strengthening.", "Forcing the limb beyond comfortable flexion.", "HipDysplasia", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 1, true, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Move slowly and stay within a pain-free range.", "Gentle hip flexion and extension to maintain joint mobility.", "Canine", "Hip flexors, hip extensors", "Passive Range of Motion", "https://commondatastorage.googleapis.com/gtv-videos-bucket/sample/ForBiggerBlazes.mp4" },
                    { 3, "Improve balance and proprioception during recovery.", "Moving too quickly between sides.", "HipDysplasia", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 2, true, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Use a non-slip surface and support the dog if needed.", "Encourage controlled lateral weight transfer over the hind limbs.", "Canine", "Core stabilisers, gluteals", "Weight Shifting", "https://commondatastorage.googleapis.com/gtv-videos-bucket/sample/ForBiggerBlazes.mp4" }
                });

            migrationBuilder.InsertData(
                table: "Pets",
                columns: new[] { "PetId", "BirthDate", "Breed", "CreatedDate", "CreatedUserId", "IsActive", "ModifiedDate", "ModifiedUserId", "OwnerId", "PetName", "Species", "WeightKg" },
                values: new object[] { 1, new DateOnly(2019, 5, 12), "Labrador Retriever", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 3, "Buddy", "Canine", 28.5m });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 3,
                column: "ClinicId",
                value: 1);

            migrationBuilder.InsertData(
                table: "ExerciseSteps",
                columns: new[] { "ExerciseStepId", "CreatedDate", "CreatedUserId", "ExerciseId", "ImageUrl", "IsActive", "ModifiedDate", "ModifiedUserId", "StepInstruction", "StepNumber" },
                values: new object[,]
                {
                    { 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 1, null, true, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Position your dog on a non-slip mat with hind limbs square.", 1 },
                    { 2, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 1, null, true, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Lure the dog into a controlled sit, keeping the spine neutral.", 2 },
                    { 3, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 1, null, true, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Cue a slow stand using a treat, pausing briefly at the top before repeating.", 3 },
                    { 4, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 2, null, true, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Support the limb gently and flex the hip slowly for 3 seconds.", 1 },
                    { 5, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 2, null, true, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Return to neutral, then extend the hip within a comfortable range.", 2 },
                    { 6, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 3, null, true, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Stand beside your dog and gently shift weight toward one hind limb.", 1 },
                    { 7, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 3, null, true, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Hold for 2 seconds, then shift to the opposite side and repeat.", 2 }
                });

            migrationBuilder.InsertData(
                table: "MedicalHistories",
                columns: new[] { "MedicalHistoryId", "ClinicianNotes", "CreatedDate", "CreatedUserId", "Diagnosis", "InjuryOrCondition", "IsActive", "ModifiedDate", "ModifiedUserId", "PetId", "SurgeryDate" },
                values: new object[] { 1, "Begin low-impact strengthening and proprioception work.", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Hip Dysplasia", "Mild bilateral hip dysplasia with reduced hind-limb mobility.", true, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 1, null });

            migrationBuilder.InsertData(
                table: "RehabPrograms",
                columns: new[] { "RehabProgramId", "CreatedDate", "CreatedUserId", "EndDate", "IsActive", "ModifiedDate", "ModifiedUserId", "Notes", "PetId", "PhysioId", "ProgramTitle", "StartDate" },
                values: new object[] { 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, true, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Low-impact introductory routine for hip dysplasia recovery.", 1, 2, "Buddy Hip Recovery - Week 1", new DateOnly(2026, 1, 1) });

            migrationBuilder.InsertData(
                table: "RehabProgramExercises",
                columns: new[] { "RehabProgramExerciseId", "CreatedDate", "CreatedUserId", "ExerciseId", "FrequencyPerDay", "IsActive", "ModifiedDate", "ModifiedUserId", "RehabProgramId", "Repetitions", "Sets" },
                values: new object[,]
                {
                    { 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 1, 2, true, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 1, 8, 3 },
                    { 2, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 2, 1, true, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 1, 10, 2 },
                    { 3, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 3, 1, true, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 1, 6, 2 }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "ExerciseSteps",
                keyColumn: "ExerciseStepId",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "ExerciseSteps",
                keyColumn: "ExerciseStepId",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "ExerciseSteps",
                keyColumn: "ExerciseStepId",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "ExerciseSteps",
                keyColumn: "ExerciseStepId",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "ExerciseSteps",
                keyColumn: "ExerciseStepId",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "ExerciseSteps",
                keyColumn: "ExerciseStepId",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "ExerciseSteps",
                keyColumn: "ExerciseStepId",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "MedicalHistories",
                keyColumn: "MedicalHistoryId",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "RehabProgramExercises",
                keyColumn: "RehabProgramExerciseId",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "RehabProgramExercises",
                keyColumn: "RehabProgramExerciseId",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "RehabProgramExercises",
                keyColumn: "RehabProgramExerciseId",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Exercises",
                keyColumn: "ExerciseId",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Exercises",
                keyColumn: "ExerciseId",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Exercises",
                keyColumn: "ExerciseId",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "RehabPrograms",
                keyColumn: "RehabProgramId",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Pets",
                keyColumn: "PetId",
                keyValue: 1);

            migrationBuilder.DropColumn(
                name: "ConditionCategory",
                table: "Exercises");

            migrationBuilder.DropColumn(
                name: "TargetSpecies",
                table: "Exercises");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 3,
                column: "ClinicId",
                value: null);
        }
    }
}
