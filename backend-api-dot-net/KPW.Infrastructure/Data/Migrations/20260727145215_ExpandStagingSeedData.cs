using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace KPW.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class ExpandStagingSeedData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Appointments",
                keyColumn: "AppointmentId",
                keyValue: 1,
                column: "ScheduledDateTime",
                value: new DateTime(2026, 7, 27, 10, 0, 0, 0, DateTimeKind.Utc));

            migrationBuilder.InsertData(
                table: "Appointments",
                columns: new[] { "AppointmentId", "AppointmentStatus", "ClientNotes", "ClinicianNotes", "CreatedDate", "CreatedUserId", "IsActive", "ModifiedDate", "ModifiedUserId", "OwnerId", "PetId", "PhysioId", "ScheduledDateTime" },
                values: new object[] { 7, "Completed", "Initial hip dysplasia assessment.", "Started sit-to-stand programme.", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 3, 1, 2, new DateTime(2026, 7, 5, 9, 30, 0, 0, DateTimeKind.Utc) });

            migrationBuilder.InsertData(
                table: "Clinics",
                columns: new[] { "ClinicId", "ClinicName", "ContactNumber", "CreatedDate", "CreatedUserId", "IsActive", "ModifiedDate", "ModifiedUserId", "PhysicalAddress", "VatNumber" },
                values: new object[] { 2, "KPW North Branch", "+27110000099", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "45 Rehabilitation Road, Centurion, Gauteng", "4987654321" });

            migrationBuilder.UpdateData(
                table: "DailyTrackingLogs",
                keyColumn: "LogId",
                keyValue: 1,
                column: "LogDate",
                value: new DateOnly(2026, 7, 13));

            migrationBuilder.UpdateData(
                table: "DailyTrackingLogs",
                keyColumn: "LogId",
                keyValue: 2,
                columns: new[] { "LogDate", "MobilityScore", "PainScore" },
                values: new object[] { new DateOnly(2026, 7, 14), 4, 7 });

            migrationBuilder.UpdateData(
                table: "DailyTrackingLogs",
                keyColumn: "LogId",
                keyValue: 3,
                columns: new[] { "EnergyScore", "LogDate", "MobilityScore" },
                values: new object[] { 5, new DateOnly(2026, 7, 15), 4 });

            migrationBuilder.UpdateData(
                table: "DailyTrackingLogs",
                keyColumn: "LogId",
                keyValue: 4,
                columns: new[] { "EnergyScore", "LogDate", "MobilityScore", "PainScore" },
                values: new object[] { 5, new DateOnly(2026, 7, 16), 5, 6 });

            migrationBuilder.UpdateData(
                table: "DailyTrackingLogs",
                keyColumn: "LogId",
                keyValue: 5,
                columns: new[] { "EnergyScore", "LogDate", "MobilityScore", "PainScore" },
                values: new object[] { 6, new DateOnly(2026, 7, 17), 5, 6 });

            migrationBuilder.UpdateData(
                table: "DailyTrackingLogs",
                keyColumn: "LogId",
                keyValue: 6,
                columns: new[] { "EnergyScore", "LogDate", "MobilityScore", "PainScore" },
                values: new object[] { 6, new DateOnly(2026, 7, 18), 5, 5 });

            migrationBuilder.UpdateData(
                table: "DailyTrackingLogs",
                keyColumn: "LogId",
                keyValue: 7,
                columns: new[] { "EnergyScore", "LogDate", "MobilityScore", "PainScore" },
                values: new object[] { 6, new DateOnly(2026, 7, 19), 6, 5 });

            migrationBuilder.InsertData(
                table: "DailyTrackingLogs",
                columns: new[] { "LogId", "AppetiteScore", "CreatedDate", "CreatedUserId", "EnergyScore", "IsActive", "IsCompleted", "LamenessScore", "LogDate", "MobilityScore", "ModifiedDate", "ModifiedUserId", "PainScore", "PetId", "WeightKg" },
                values: new object[,]
                {
                    { 8, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 6, true, true, null, new DateOnly(2026, 7, 20), 6, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 5, 1, null },
                    { 9, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 7, true, true, null, new DateOnly(2026, 7, 21), 6, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 5, 1, null },
                    { 10, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 7, true, true, null, new DateOnly(2026, 7, 22), 6, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 4, 1, null },
                    { 11, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 7, true, true, null, new DateOnly(2026, 7, 23), 7, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 4, 1, null },
                    { 12, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 7, true, true, null, new DateOnly(2026, 7, 24), 7, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 4, 1, null },
                    { 13, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 8, true, true, null, new DateOnly(2026, 7, 25), 7, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 4, 1, null },
                    { 14, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 8, true, true, null, new DateOnly(2026, 7, 26), 7, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 4, 1, null }
                });

            migrationBuilder.InsertData(
                table: "ExerciseSessionLogs",
                columns: new[] { "ExerciseSessionLogId", "CompletedAt", "CreatedDate", "CreatedUserId", "ExerciseId", "IsActive", "ModifiedDate", "ModifiedUserId", "PetId", "RehabProgramId" },
                values: new object[,]
                {
                    { 1, new DateTime(2026, 7, 27, 8, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 1, true, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 1, 1 },
                    { 2, new DateTime(2026, 7, 27, 8, 30, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 2, true, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 1, 1 },
                    { 3, new DateTime(2026, 7, 26, 9, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 3, true, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 1, 1 },
                    { 12, new DateTime(2026, 7, 26, 8, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 1, true, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 1, 1 },
                    { 13, new DateTime(2026, 7, 26, 17, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 1, true, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 1, 1 }
                });

            migrationBuilder.InsertData(
                table: "Exercises",
                columns: new[] { "ExerciseId", "ClinicalPurpose", "CommonMistakes", "ConditionCategory", "CreatedDate", "CreatedUserId", "DifficultyLevel", "IsActive", "ModifiedDate", "ModifiedUserId", "SafetyNotes", "ShortDescription", "TargetSpecies", "TargetedMuscles", "Title", "VideoUrl" },
                values: new object[,]
                {
                    { 4, "Rebuild neuromuscular control after CCL surgery.", "Poles set too high or spaced inconsistently.", "PostOperative", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 2, true, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Keep pole height low; stop if limping worsens.", "Low obstacle walking to restore stifle stability and coordination.", "Canine", "Quadriceps, hamstrings, core", "Cavaletti Poles", "https://commondatastorage.googleapis.com/gtv-videos-bucket/sample/ForBiggerBlazes.mp4" },
                    { 5, "Gradual return to weight-bearing after lameness.", "Allowing the dog to pull or trot before ready.", "Lameness", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 1, true, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Avoid slippery floors; keep sessions short.", "Controlled leash walking on varied surfaces.", "Canine", "Forelimb stabilisers, shoulder girdle", "Slow Lead Walk", "https://commondatastorage.googleapis.com/gtv-videos-bucket/sample/ForBiggerBlazes.mp4" },
                    { 6, "Support weight management and mobility improvement.", "Choosing slopes that are too steep.", "WeightManagement", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 2, true, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Use a mild incline only; monitor breathing rate.", "Gentle uphill walking to build endurance without high impact.", "Canine", "Hind-limb extensors, cardiovascular system", "Incline Walk", "https://commondatastorage.googleapis.com/gtv-videos-bucket/sample/ForBiggerBlazes.mp4" },
                    { 7, "Maintain joint mobility in arthritic cats.", "Restraining too firmly causing stress.", "Arthritis", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 1, true, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Keep sessions under 5 minutes; reward calm behaviour.", "Slow elbow and shoulder flexion for feline arthritis.", "Feline", "Shoulder flexors, elbow extensors", "Gentle Stretch", "https://commondatastorage.googleapis.com/gtv-videos-bucket/sample/ForBiggerBlazes.mp4" },
                    { 8, "Safe reintroduction to movement after abdominal surgery.", "Rushing the transition before the incision has healed.", "PostOperative", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 1, true, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "No jumping onto or off furniture.", "Controlled in-and-out crate movements post-surgery.", "Feline", "Core, hind-limb flexors", "Crate Rest Transitions", "https://commondatastorage.googleapis.com/gtv-videos-bucket/sample/ForBiggerBlazes.mp4" }
                });

            migrationBuilder.InsertData(
                table: "MedicalHistories",
                columns: new[] { "MedicalHistoryId", "ClinicianNotes", "CreatedDate", "CreatedUserId", "Diagnosis", "InjuryOrCondition", "IsActive", "ModifiedDate", "ModifiedUserId", "PetId", "SurgeryDate" },
                values: new object[] { 7, "Monitor during hind-limb loading exercises.", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Prior Elbow Dysplasia", "Historical mild elbow dysplasia, managed conservatively since 2024.", true, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 1, null });

            migrationBuilder.InsertData(
                table: "MessageThreads",
                columns: new[] { "MessageThreadId", "CreatedDate", "CreatedUserId", "IsActive", "ModifiedDate", "ModifiedUserId", "OwnerId", "PetId", "PhysioId" },
                values: new object[] { 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 3, 1, 2 });

            migrationBuilder.InsertData(
                table: "Pets",
                columns: new[] { "PetId", "BirthDate", "Breed", "CreatedDate", "CreatedUserId", "IsActive", "ModifiedDate", "ModifiedUserId", "OwnerId", "PetName", "Species", "WeightKg" },
                values: new object[,]
                {
                    { 2, new DateOnly(2020, 3, 8), "Border Collie", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 3, "Luna", "Canine", 18.2m },
                    { 3, new DateOnly(2018, 11, 22), "German Shepherd", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 3, "Max", "Canine", 34.0m },
                    { 4, new DateOnly(2017, 7, 4), "Beagle", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 3, "Bella", "Canine", 14.8m },
                    { 5, new DateOnly(2016, 2, 14), "Domestic Shorthair", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 3, "Whiskers", "Feline", 4.6m },
                    { 6, new DateOnly(2021, 9, 30), "Maine Coon", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 3, "Milo", "Feline", 5.9m }
                });

            migrationBuilder.UpdateData(
                table: "RehabPrograms",
                keyColumn: "RehabProgramId",
                keyValue: 1,
                column: "ProgramTitle",
                value: "Buddy Hip Recovery - Week 4");

            migrationBuilder.InsertData(
                table: "VideoSubmissions",
                columns: new[] { "VideoSubmissionId", "CreatedDate", "CreatedUserId", "ExerciseId", "IsActive", "IsReviewed", "ModifiedDate", "ModifiedUserId", "PetId", "PhysioFeedbackNotes", "ProcessedVideoStreamingUrl", "ProcessingStatus", "RawVideoStorageUrl" },
                values: new object[] { 2, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 3, true, true, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 1, "Good weight transfer — try holding each side for 3 seconds instead of 2.", "https://commondatastorage.googleapis.com/gtv-videos-bucket/sample/ForBiggerBlazes.mp4", "Ready", "videos/demo-buddy-weight-shift-raw.mp4" });

            migrationBuilder.InsertData(
                table: "Appointments",
                columns: new[] { "AppointmentId", "AppointmentStatus", "ClientNotes", "ClinicianNotes", "CreatedDate", "CreatedUserId", "IsActive", "ModifiedDate", "ModifiedUserId", "OwnerId", "PetId", "PhysioId", "ScheduledDateTime" },
                values: new object[,]
                {
                    { 2, "Scheduled", "ACL recovery check — stifle stability assessment.", null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 3, 2, 2, new DateTime(2026, 7, 27, 14, 0, 0, 0, DateTimeKind.Utc) },
                    { 3, "Scheduled", "Review lameness improvement this week.", null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 3, 3, 2, new DateTime(2026, 7, 27, 18, 0, 0, 0, DateTimeKind.Utc) },
                    { 4, "Completed", "Weight check and mobility review.", "Weight stable; continue incline walks.", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 3, 4, 2, new DateTime(2026, 7, 20, 9, 0, 0, 0, DateTimeKind.Utc) },
                    { 5, "Completed", "Arthritis management review.", "Good response to gentle stretches.", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 3, 5, 2, new DateTime(2026, 7, 15, 11, 0, 0, 0, DateTimeKind.Utc) },
                    { 6, "Cancelled", "Post-op check — rescheduled due to travel.", null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 3, 6, 2, new DateTime(2026, 7, 10, 10, 0, 0, 0, DateTimeKind.Utc) },
                    { 8, "Scheduled", "4-week post-surgery milestone review.", null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 3, 2, 2, new DateTime(2026, 7, 30, 10, 0, 0, 0, DateTimeKind.Utc) }
                });

            migrationBuilder.InsertData(
                table: "DailyTrackingLogs",
                columns: new[] { "LogId", "AppetiteScore", "CreatedDate", "CreatedUserId", "EnergyScore", "IsActive", "IsCompleted", "LamenessScore", "LogDate", "MobilityScore", "ModifiedDate", "ModifiedUserId", "PainScore", "PetId", "WeightKg" },
                values: new object[,]
                {
                    { 15, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 4, true, true, null, new DateOnly(2026, 7, 13), 3, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 8, 2, null },
                    { 16, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 4, true, true, null, new DateOnly(2026, 7, 14), 3, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 7, 2, null },
                    { 17, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 5, true, true, null, new DateOnly(2026, 7, 15), 4, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 7, 2, null },
                    { 18, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 5, true, true, null, new DateOnly(2026, 7, 16), 4, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 6, 2, null },
                    { 19, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 5, true, true, null, new DateOnly(2026, 7, 17), 4, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 6, 2, null },
                    { 20, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 6, true, true, null, new DateOnly(2026, 7, 18), 5, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 6, 2, null },
                    { 21, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 6, true, true, null, new DateOnly(2026, 7, 19), 5, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 5, 2, null },
                    { 22, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 6, true, true, null, new DateOnly(2026, 7, 20), 5, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 5, 2, null },
                    { 23, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 7, true, true, null, new DateOnly(2026, 7, 21), 6, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 5, 2, null },
                    { 24, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 7, true, true, null, new DateOnly(2026, 7, 22), 6, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 4, 2, null },
                    { 25, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 7, true, true, null, new DateOnly(2026, 7, 23), 6, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 4, 2, null },
                    { 26, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 8, true, true, null, new DateOnly(2026, 7, 24), 7, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 4, 2, null },
                    { 27, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 8, true, true, null, new DateOnly(2026, 7, 25), 7, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 3, 2, null },
                    { 28, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 8, true, true, null, new DateOnly(2026, 7, 26), 8, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 3, 2, null },
                    { 29, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 6, true, true, null, new DateOnly(2026, 7, 13), 5, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 6, 3, null },
                    { 30, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 6, true, true, null, new DateOnly(2026, 7, 14), 5, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 6, 3, null },
                    { 31, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 6, true, true, null, new DateOnly(2026, 7, 15), 5, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 5, 3, null },
                    { 32, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 7, true, true, null, new DateOnly(2026, 7, 16), 5, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 5, 3, null },
                    { 33, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 7, true, true, null, new DateOnly(2026, 7, 17), 6, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 5, 3, null },
                    { 34, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 7, true, true, null, new DateOnly(2026, 7, 18), 6, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 5, 3, null },
                    { 35, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 7, true, true, null, new DateOnly(2026, 7, 19), 6, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 4, 3, null },
                    { 36, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 7, true, true, null, new DateOnly(2026, 7, 20), 6, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 4, 3, null },
                    { 37, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 8, true, true, null, new DateOnly(2026, 7, 21), 7, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 4, 3, null },
                    { 38, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 8, true, true, null, new DateOnly(2026, 7, 22), 7, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 4, 3, null },
                    { 39, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 8, true, true, null, new DateOnly(2026, 7, 23), 7, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 3, 3, null },
                    { 40, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 8, true, true, null, new DateOnly(2026, 7, 24), 7, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 3, 3, null },
                    { 41, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 8, true, true, null, new DateOnly(2026, 7, 25), 8, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 3, 3, null },
                    { 42, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 9, true, true, null, new DateOnly(2026, 7, 26), 8, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 3, 3, null }
                });

            migrationBuilder.InsertData(
                table: "ExerciseSteps",
                columns: new[] { "ExerciseStepId", "CreatedDate", "CreatedUserId", "ExerciseId", "ImageUrl", "IsActive", "ModifiedDate", "ModifiedUserId", "StepInstruction", "StepNumber" },
                values: new object[,]
                {
                    { 8, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 4, null, true, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Set 3–4 poles at low height, spaced to match your dog's stride.", 1 },
                    { 9, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 4, null, true, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Walk slowly through the poles on a loose leash, rewarding calm steps.", 2 },
                    { 10, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 4, null, true, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Complete 3 passes, rest 30 seconds, then repeat for 2 sets.", 3 },
                    { 11, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 5, null, true, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Begin on a flat, non-slip surface with a short leash.", 1 },
                    { 12, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 5, null, true, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Walk at a slow pace for 5 minutes, encouraging even weight distribution.", 2 },
                    { 13, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 6, null, true, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Find a gentle incline (5–10 degrees) with good footing.", 1 },
                    { 14, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 6, null, true, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Walk uphill for 2 minutes, rest, then walk down slowly.", 2 },
                    { 15, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 7, null, true, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Place your cat on a comfortable surface and allow them to settle.", 1 },
                    { 16, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 7, null, true, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Gently flex each front limb, holding for 3 seconds within a pain-free range.", 2 },
                    { 17, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 8, null, true, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Open the crate door and lure your cat out with a treat at ground level.", 1 },
                    { 18, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 8, null, true, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Guide them back in slowly; repeat 5 times with rest between.", 2 },
                    { 19, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 8, null, true, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "End the session before your cat shows signs of fatigue or stress.", 3 }
                });

            migrationBuilder.InsertData(
                table: "MedicalHistories",
                columns: new[] { "MedicalHistoryId", "ClinicianNotes", "CreatedDate", "CreatedUserId", "Diagnosis", "InjuryOrCondition", "IsActive", "ModifiedDate", "ModifiedUserId", "PetId", "SurgeryDate" },
                values: new object[,]
                {
                    { 2, "Progress to controlled loading; monitor for limb favouring.", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Cranial Cruciate Ligament Rupture", "Right stifle CCL rupture, post-surgical repair 3 weeks ago.", true, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 2, null },
                    { 3, "Focus on proprioception and gradual return to activity.", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Chronic Lameness", "Intermittent forelimb lameness, suspected soft tissue strain.", true, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 3, null },
                    { 4, "Combine weight management with low-impact mobility exercises.", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Obesity-Related Mobility Decline", "Overweight with reduced exercise tolerance and stiff gait.", true, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 4, null },
                    { 5, "Gentle range-of-motion and environmental modification advised.", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Feline Osteoarthritis", "Bilateral elbow osteoarthritis with reduced jumping ability.", true, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 5, null },
                    { 6, "Gradual return to movement; avoid jumping for 4 weeks.", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Post-Operative Recovery", "Abdominal surgery 10 days ago; restricted activity period.", true, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 6, null }
                });

            migrationBuilder.InsertData(
                table: "MessageThreads",
                columns: new[] { "MessageThreadId", "CreatedDate", "CreatedUserId", "IsActive", "ModifiedDate", "ModifiedUserId", "OwnerId", "PetId", "PhysioId" },
                values: new object[,]
                {
                    { 2, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 3, 2, 2 },
                    { 3, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 3, 3, 2 },
                    { 4, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 3, 4, 2 },
                    { 5, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 3, 5, 2 }
                });

            migrationBuilder.InsertData(
                table: "Messages",
                columns: new[] { "MessageId", "Body", "CreatedDate", "CreatedUserId", "IsActive", "MessageThreadId", "ModifiedDate", "ModifiedUserId", "ReadAt", "SenderUserId", "VideoSubmissionId" },
                values: new object[,]
                {
                    { 1, "Hi, Buddy seems a bit stiff after yesterday's sit-to-stand session. Is that normal?", new DateTime(2026, 7, 24, 0, 0, 0, 0, DateTimeKind.Utc), null, true, 1, new DateTime(2026, 7, 24, 0, 0, 0, 0, DateTimeKind.Utc), null, null, 3, null },
                    { 2, "Some mild stiffness is expected in week 4. If it persists beyond 24 hours or he won't bear weight, let me know.", new DateTime(2026, 7, 24, 1, 0, 0, 0, DateTimeKind.Utc), null, true, 1, new DateTime(2026, 7, 24, 1, 0, 0, 0, DateTimeKind.Utc), null, new DateTime(2026, 7, 24, 2, 0, 0, 0, DateTimeKind.Utc), 2, null },
                    { 3, "Thanks — he's much better this morning. I'll upload today's video shortly.", new DateTime(2026, 7, 24, 3, 0, 0, 0, DateTimeKind.Utc), null, true, 1, new DateTime(2026, 7, 24, 3, 0, 0, 0, DateTimeKind.Utc), null, new DateTime(2026, 7, 24, 4, 0, 0, 0, DateTimeKind.Utc), 3, null },
                    { 4, "Uploaded the weight-shifting video — let me know what you think!", new DateTime(2026, 7, 26, 0, 0, 0, 0, DateTimeKind.Utc), null, true, 1, new DateTime(2026, 7, 26, 0, 0, 0, 0, DateTimeKind.Utc), null, null, 3, 2 },
                    { 17, "Reviewed Buddy's weight-shifting video — nice improvement from last week.", new DateTime(2026, 7, 27, 0, 0, 0, 0, DateTimeKind.Utc), null, true, 1, new DateTime(2026, 7, 27, 0, 0, 0, 0, DateTimeKind.Utc), null, null, 2, null }
                });

            migrationBuilder.InsertData(
                table: "RehabPrograms",
                columns: new[] { "RehabProgramId", "CreatedDate", "CreatedUserId", "EndDate", "IsActive", "ModifiedDate", "ModifiedUserId", "Notes", "PetId", "PhysioId", "ProgramTitle", "StartDate" },
                values: new object[,]
                {
                    { 2, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, true, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Post-surgical stifle rehabilitation with controlled loading.", 2, 2, "Luna ACL Recovery - Week 3", new DateOnly(2026, 7, 6) },
                    { 3, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, true, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Proprioception and gradual return to activity.", 3, 2, "Max Lameness Rehab", new DateOnly(2026, 7, 1) },
                    { 4, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, true, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Combined weight management and low-impact exercise.", 4, 2, "Bella Weight & Mobility Plan", new DateOnly(2026, 6, 15) },
                    { 5, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, true, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Gentle feline mobility programme.", 5, 2, "Whiskers Arthritis Care", new DateOnly(2026, 6, 1) }
                });

            migrationBuilder.InsertData(
                table: "VideoSubmissions",
                columns: new[] { "VideoSubmissionId", "CreatedDate", "CreatedUserId", "ExerciseId", "IsActive", "IsReviewed", "ModifiedDate", "ModifiedUserId", "PetId", "PhysioFeedbackNotes", "ProcessedVideoStreamingUrl", "RawVideoStorageUrl" },
                values: new object[] { 3, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 4, true, false, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 2, null, null, "videos/demo-luna-cavaletti-raw.mp4" });

            migrationBuilder.InsertData(
                table: "VideoSubmissions",
                columns: new[] { "VideoSubmissionId", "CreatedDate", "CreatedUserId", "ExerciseId", "IsActive", "IsReviewed", "ModifiedDate", "ModifiedUserId", "PetId", "PhysioFeedbackNotes", "ProcessedVideoStreamingUrl", "ProcessingStatus", "RawVideoStorageUrl" },
                values: new object[] { 4, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 4, true, true, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 2, "Excellent pole clearance. Increase to 4 passes next week.", "https://commondatastorage.googleapis.com/gtv-videos-bucket/sample/ForBiggerBlazes.mp4", "Ready", "videos/demo-luna-cavaletti-week2-raw.mp4" });

            migrationBuilder.InsertData(
                table: "VideoSubmissions",
                columns: new[] { "VideoSubmissionId", "CreatedDate", "CreatedUserId", "ExerciseId", "IsActive", "IsReviewed", "ModifiedDate", "ModifiedUserId", "PetId", "PhysioFeedbackNotes", "ProcessedVideoStreamingUrl", "RawVideoStorageUrl" },
                values: new object[] { 5, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 5, true, false, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 3, null, null, "videos/demo-max-lead-walk-raw.mp4" });

            migrationBuilder.InsertData(
                table: "VideoSubmissions",
                columns: new[] { "VideoSubmissionId", "CreatedDate", "CreatedUserId", "ExerciseId", "IsActive", "IsReviewed", "ModifiedDate", "ModifiedUserId", "PetId", "PhysioFeedbackNotes", "ProcessedVideoStreamingUrl", "ProcessingStatus", "RawVideoStorageUrl" },
                values: new object[,]
                {
                    { 6, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 6, true, false, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 4, null, null, "Processing", "videos/demo-bella-incline-raw.mp4" },
                    { 7, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 7, true, false, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 5, null, null, "Failed", "videos/demo-whiskers-stretch-raw.mp4" }
                });

            migrationBuilder.InsertData(
                table: "ExerciseSessionLogs",
                columns: new[] { "ExerciseSessionLogId", "CompletedAt", "CreatedDate", "CreatedUserId", "ExerciseId", "IsActive", "ModifiedDate", "ModifiedUserId", "PetId", "RehabProgramId" },
                values: new object[,]
                {
                    { 4, new DateTime(2026, 7, 27, 7, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 4, true, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 2, 2 },
                    { 5, new DateTime(2026, 7, 26, 10, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 5, true, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 2, 2 },
                    { 6, new DateTime(2026, 7, 27, 6, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 5, true, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 3, 3 },
                    { 7, new DateTime(2026, 7, 26, 11, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 3, true, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 3, 3 },
                    { 8, new DateTime(2026, 7, 27, 7, 30, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 6, true, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 4, 4 },
                    { 9, new DateTime(2026, 7, 25, 9, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 5, true, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 4, 4 },
                    { 10, new DateTime(2026, 7, 27, 9, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 7, true, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 5, 5 },
                    { 11, new DateTime(2026, 7, 26, 8, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 8, true, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 5, 5 },
                    { 14, new DateTime(2026, 7, 26, 7, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 4, true, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 2, 2 },
                    { 15, new DateTime(2026, 7, 26, 16, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 4, true, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 2, 2 }
                });

            migrationBuilder.InsertData(
                table: "Messages",
                columns: new[] { "MessageId", "Body", "CreatedDate", "CreatedUserId", "IsActive", "MessageThreadId", "ModifiedDate", "ModifiedUserId", "ReadAt", "SenderUserId", "VideoSubmissionId" },
                values: new object[,]
                {
                    { 5, "Luna's recovery is tracking well. Ready to add cavaletti poles this week.", new DateTime(2026, 7, 22, 0, 0, 0, 0, DateTimeKind.Utc), null, true, 2, new DateTime(2026, 7, 22, 0, 0, 0, 0, DateTimeKind.Utc), null, new DateTime(2026, 7, 24, 0, 0, 0, 0, DateTimeKind.Utc), 2, null },
                    { 6, "Great! She's eager to work — I'll start with the lowest pole height.", new DateTime(2026, 7, 22, 3, 0, 0, 0, DateTimeKind.Utc), null, true, 2, new DateTime(2026, 7, 22, 3, 0, 0, 0, DateTimeKind.Utc), null, new DateTime(2026, 7, 24, 1, 0, 0, 0, DateTimeKind.Utc), 3, null },
                    { 7, "Cavaletti video uploaded. She knocked one pole on the third pass.", new DateTime(2026, 7, 26, 0, 0, 0, 0, DateTimeKind.Utc), null, true, 2, new DateTime(2026, 7, 26, 0, 0, 0, 0, DateTimeKind.Utc), null, null, 3, 3 },
                    { 8, "That's fine for week 3 — spacing looks good. I'll review the video today.", new DateTime(2026, 7, 27, 0, 0, 0, 0, DateTimeKind.Utc), null, true, 2, new DateTime(2026, 7, 27, 0, 0, 0, 0, DateTimeKind.Utc), null, null, 2, null },
                    { 9, "Max is still favouring his left front leg on walks. Should I reduce the duration?", new DateTime(2026, 7, 23, 0, 0, 0, 0, DateTimeKind.Utc), null, true, 3, new DateTime(2026, 7, 23, 0, 0, 0, 0, DateTimeKind.Utc), null, null, 3, null },
                    { 10, "Yes, drop to 3 minutes for now and keep surfaces flat. Upload a walk video if you can.", new DateTime(2026, 7, 24, 0, 0, 0, 0, DateTimeKind.Utc), null, true, 3, new DateTime(2026, 7, 24, 0, 0, 0, 0, DateTimeKind.Utc), null, new DateTime(2026, 7, 24, 0, 0, 0, 0, DateTimeKind.Utc), 2, null },
                    { 11, "Will do — thanks for the quick reply.", new DateTime(2026, 7, 24, 1, 0, 0, 0, DateTimeKind.Utc), null, true, 3, new DateTime(2026, 7, 24, 1, 0, 0, 0, DateTimeKind.Utc), null, new DateTime(2026, 7, 24, 1, 0, 0, 0, DateTimeKind.Utc), 3, null },
                    { 12, "Bella's weight has dropped 0.3 kg since last visit. Keep up the incline walks.", new DateTime(2026, 7, 20, 0, 0, 0, 0, DateTimeKind.Utc), null, true, 4, new DateTime(2026, 7, 20, 0, 0, 0, 0, DateTimeKind.Utc), null, new DateTime(2026, 7, 21, 0, 0, 0, 0, DateTimeKind.Utc), 2, null },
                    { 13, "She's enjoying the walks! Energy seems higher too.", new DateTime(2026, 7, 21, 0, 0, 0, 0, DateTimeKind.Utc), null, true, 4, new DateTime(2026, 7, 21, 0, 0, 0, 0, DateTimeKind.Utc), null, new DateTime(2026, 7, 22, 0, 0, 0, 0, DateTimeKind.Utc), 3, null },
                    { 14, "Whiskers won't stay still for the stretches. Any tips?", new DateTime(2026, 7, 22, 0, 0, 0, 0, DateTimeKind.Utc), null, true, 5, new DateTime(2026, 7, 22, 0, 0, 0, 0, DateTimeKind.Utc), null, null, 3, null },
                    { 15, "Try shorter sessions with treats after each limb. Feliway spray can help too.", new DateTime(2026, 7, 22, 4, 0, 0, 0, DateTimeKind.Utc), null, true, 5, new DateTime(2026, 7, 22, 4, 0, 0, 0, DateTimeKind.Utc), null, new DateTime(2026, 7, 23, 0, 0, 0, 0, DateTimeKind.Utc), 2, null },
                    { 16, "That worked much better — she completed all 5 stretches today!", new DateTime(2026, 7, 26, 0, 0, 0, 0, DateTimeKind.Utc), null, true, 5, new DateTime(2026, 7, 26, 0, 0, 0, 0, DateTimeKind.Utc), null, new DateTime(2026, 7, 26, 0, 0, 0, 0, DateTimeKind.Utc), 3, null },
                    { 18, "Uploaded an incline walk video for Bella.", new DateTime(2026, 7, 27, 0, 0, 0, 0, DateTimeKind.Utc), null, true, 4, new DateTime(2026, 7, 27, 0, 0, 0, 0, DateTimeKind.Utc), null, null, 3, 6 }
                });

            migrationBuilder.InsertData(
                table: "RehabProgramExercises",
                columns: new[] { "RehabProgramExerciseId", "CreatedDate", "CreatedUserId", "ExerciseId", "FrequencyPerDay", "IsActive", "ModifiedDate", "ModifiedUserId", "RehabProgramId", "Repetitions", "Sets" },
                values: new object[,]
                {
                    { 4, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 4, 2, true, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 2, 3, 2 },
                    { 5, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 5, 1, true, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 2, 1, 1 },
                    { 6, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 5, 2, true, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 3, 1, 1 },
                    { 7, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 3, 1, true, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 3, 6, 2 },
                    { 8, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 6, 1, true, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 4, 1, 1 },
                    { 9, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 5, 1, true, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 4, 1, 1 },
                    { 10, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 7, 2, true, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 5, 5, 2 },
                    { 11, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 8, 1, true, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 5, 5, 1 },
                    { 12, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 3, 1, true, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 2, 6, 2 }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Appointments",
                keyColumn: "AppointmentId",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Appointments",
                keyColumn: "AppointmentId",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Appointments",
                keyColumn: "AppointmentId",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Appointments",
                keyColumn: "AppointmentId",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "Appointments",
                keyColumn: "AppointmentId",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "Appointments",
                keyColumn: "AppointmentId",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "Appointments",
                keyColumn: "AppointmentId",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "Clinics",
                keyColumn: "ClinicId",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "DailyTrackingLogs",
                keyColumn: "LogId",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "DailyTrackingLogs",
                keyColumn: "LogId",
                keyValue: 9);

            migrationBuilder.DeleteData(
                table: "DailyTrackingLogs",
                keyColumn: "LogId",
                keyValue: 10);

            migrationBuilder.DeleteData(
                table: "DailyTrackingLogs",
                keyColumn: "LogId",
                keyValue: 11);

            migrationBuilder.DeleteData(
                table: "DailyTrackingLogs",
                keyColumn: "LogId",
                keyValue: 12);

            migrationBuilder.DeleteData(
                table: "DailyTrackingLogs",
                keyColumn: "LogId",
                keyValue: 13);

            migrationBuilder.DeleteData(
                table: "DailyTrackingLogs",
                keyColumn: "LogId",
                keyValue: 14);

            migrationBuilder.DeleteData(
                table: "DailyTrackingLogs",
                keyColumn: "LogId",
                keyValue: 15);

            migrationBuilder.DeleteData(
                table: "DailyTrackingLogs",
                keyColumn: "LogId",
                keyValue: 16);

            migrationBuilder.DeleteData(
                table: "DailyTrackingLogs",
                keyColumn: "LogId",
                keyValue: 17);

            migrationBuilder.DeleteData(
                table: "DailyTrackingLogs",
                keyColumn: "LogId",
                keyValue: 18);

            migrationBuilder.DeleteData(
                table: "DailyTrackingLogs",
                keyColumn: "LogId",
                keyValue: 19);

            migrationBuilder.DeleteData(
                table: "DailyTrackingLogs",
                keyColumn: "LogId",
                keyValue: 20);

            migrationBuilder.DeleteData(
                table: "DailyTrackingLogs",
                keyColumn: "LogId",
                keyValue: 21);

            migrationBuilder.DeleteData(
                table: "DailyTrackingLogs",
                keyColumn: "LogId",
                keyValue: 22);

            migrationBuilder.DeleteData(
                table: "DailyTrackingLogs",
                keyColumn: "LogId",
                keyValue: 23);

            migrationBuilder.DeleteData(
                table: "DailyTrackingLogs",
                keyColumn: "LogId",
                keyValue: 24);

            migrationBuilder.DeleteData(
                table: "DailyTrackingLogs",
                keyColumn: "LogId",
                keyValue: 25);

            migrationBuilder.DeleteData(
                table: "DailyTrackingLogs",
                keyColumn: "LogId",
                keyValue: 26);

            migrationBuilder.DeleteData(
                table: "DailyTrackingLogs",
                keyColumn: "LogId",
                keyValue: 27);

            migrationBuilder.DeleteData(
                table: "DailyTrackingLogs",
                keyColumn: "LogId",
                keyValue: 28);

            migrationBuilder.DeleteData(
                table: "DailyTrackingLogs",
                keyColumn: "LogId",
                keyValue: 29);

            migrationBuilder.DeleteData(
                table: "DailyTrackingLogs",
                keyColumn: "LogId",
                keyValue: 30);

            migrationBuilder.DeleteData(
                table: "DailyTrackingLogs",
                keyColumn: "LogId",
                keyValue: 31);

            migrationBuilder.DeleteData(
                table: "DailyTrackingLogs",
                keyColumn: "LogId",
                keyValue: 32);

            migrationBuilder.DeleteData(
                table: "DailyTrackingLogs",
                keyColumn: "LogId",
                keyValue: 33);

            migrationBuilder.DeleteData(
                table: "DailyTrackingLogs",
                keyColumn: "LogId",
                keyValue: 34);

            migrationBuilder.DeleteData(
                table: "DailyTrackingLogs",
                keyColumn: "LogId",
                keyValue: 35);

            migrationBuilder.DeleteData(
                table: "DailyTrackingLogs",
                keyColumn: "LogId",
                keyValue: 36);

            migrationBuilder.DeleteData(
                table: "DailyTrackingLogs",
                keyColumn: "LogId",
                keyValue: 37);

            migrationBuilder.DeleteData(
                table: "DailyTrackingLogs",
                keyColumn: "LogId",
                keyValue: 38);

            migrationBuilder.DeleteData(
                table: "DailyTrackingLogs",
                keyColumn: "LogId",
                keyValue: 39);

            migrationBuilder.DeleteData(
                table: "DailyTrackingLogs",
                keyColumn: "LogId",
                keyValue: 40);

            migrationBuilder.DeleteData(
                table: "DailyTrackingLogs",
                keyColumn: "LogId",
                keyValue: 41);

            migrationBuilder.DeleteData(
                table: "DailyTrackingLogs",
                keyColumn: "LogId",
                keyValue: 42);

            migrationBuilder.DeleteData(
                table: "ExerciseSessionLogs",
                keyColumn: "ExerciseSessionLogId",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "ExerciseSessionLogs",
                keyColumn: "ExerciseSessionLogId",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "ExerciseSessionLogs",
                keyColumn: "ExerciseSessionLogId",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "ExerciseSessionLogs",
                keyColumn: "ExerciseSessionLogId",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "ExerciseSessionLogs",
                keyColumn: "ExerciseSessionLogId",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "ExerciseSessionLogs",
                keyColumn: "ExerciseSessionLogId",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "ExerciseSessionLogs",
                keyColumn: "ExerciseSessionLogId",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "ExerciseSessionLogs",
                keyColumn: "ExerciseSessionLogId",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "ExerciseSessionLogs",
                keyColumn: "ExerciseSessionLogId",
                keyValue: 9);

            migrationBuilder.DeleteData(
                table: "ExerciseSessionLogs",
                keyColumn: "ExerciseSessionLogId",
                keyValue: 10);

            migrationBuilder.DeleteData(
                table: "ExerciseSessionLogs",
                keyColumn: "ExerciseSessionLogId",
                keyValue: 11);

            migrationBuilder.DeleteData(
                table: "ExerciseSessionLogs",
                keyColumn: "ExerciseSessionLogId",
                keyValue: 12);

            migrationBuilder.DeleteData(
                table: "ExerciseSessionLogs",
                keyColumn: "ExerciseSessionLogId",
                keyValue: 13);

            migrationBuilder.DeleteData(
                table: "ExerciseSessionLogs",
                keyColumn: "ExerciseSessionLogId",
                keyValue: 14);

            migrationBuilder.DeleteData(
                table: "ExerciseSessionLogs",
                keyColumn: "ExerciseSessionLogId",
                keyValue: 15);

            migrationBuilder.DeleteData(
                table: "ExerciseSteps",
                keyColumn: "ExerciseStepId",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "ExerciseSteps",
                keyColumn: "ExerciseStepId",
                keyValue: 9);

            migrationBuilder.DeleteData(
                table: "ExerciseSteps",
                keyColumn: "ExerciseStepId",
                keyValue: 10);

            migrationBuilder.DeleteData(
                table: "ExerciseSteps",
                keyColumn: "ExerciseStepId",
                keyValue: 11);

            migrationBuilder.DeleteData(
                table: "ExerciseSteps",
                keyColumn: "ExerciseStepId",
                keyValue: 12);

            migrationBuilder.DeleteData(
                table: "ExerciseSteps",
                keyColumn: "ExerciseStepId",
                keyValue: 13);

            migrationBuilder.DeleteData(
                table: "ExerciseSteps",
                keyColumn: "ExerciseStepId",
                keyValue: 14);

            migrationBuilder.DeleteData(
                table: "ExerciseSteps",
                keyColumn: "ExerciseStepId",
                keyValue: 15);

            migrationBuilder.DeleteData(
                table: "ExerciseSteps",
                keyColumn: "ExerciseStepId",
                keyValue: 16);

            migrationBuilder.DeleteData(
                table: "ExerciseSteps",
                keyColumn: "ExerciseStepId",
                keyValue: 17);

            migrationBuilder.DeleteData(
                table: "ExerciseSteps",
                keyColumn: "ExerciseStepId",
                keyValue: 18);

            migrationBuilder.DeleteData(
                table: "ExerciseSteps",
                keyColumn: "ExerciseStepId",
                keyValue: 19);

            migrationBuilder.DeleteData(
                table: "MedicalHistories",
                keyColumn: "MedicalHistoryId",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "MedicalHistories",
                keyColumn: "MedicalHistoryId",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "MedicalHistories",
                keyColumn: "MedicalHistoryId",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "MedicalHistories",
                keyColumn: "MedicalHistoryId",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "MedicalHistories",
                keyColumn: "MedicalHistoryId",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "MedicalHistories",
                keyColumn: "MedicalHistoryId",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "Messages",
                keyColumn: "MessageId",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Messages",
                keyColumn: "MessageId",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Messages",
                keyColumn: "MessageId",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Messages",
                keyColumn: "MessageId",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Messages",
                keyColumn: "MessageId",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "Messages",
                keyColumn: "MessageId",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "Messages",
                keyColumn: "MessageId",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "Messages",
                keyColumn: "MessageId",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "Messages",
                keyColumn: "MessageId",
                keyValue: 9);

            migrationBuilder.DeleteData(
                table: "Messages",
                keyColumn: "MessageId",
                keyValue: 10);

            migrationBuilder.DeleteData(
                table: "Messages",
                keyColumn: "MessageId",
                keyValue: 11);

            migrationBuilder.DeleteData(
                table: "Messages",
                keyColumn: "MessageId",
                keyValue: 12);

            migrationBuilder.DeleteData(
                table: "Messages",
                keyColumn: "MessageId",
                keyValue: 13);

            migrationBuilder.DeleteData(
                table: "Messages",
                keyColumn: "MessageId",
                keyValue: 14);

            migrationBuilder.DeleteData(
                table: "Messages",
                keyColumn: "MessageId",
                keyValue: 15);

            migrationBuilder.DeleteData(
                table: "Messages",
                keyColumn: "MessageId",
                keyValue: 16);

            migrationBuilder.DeleteData(
                table: "Messages",
                keyColumn: "MessageId",
                keyValue: 17);

            migrationBuilder.DeleteData(
                table: "Messages",
                keyColumn: "MessageId",
                keyValue: 18);

            migrationBuilder.DeleteData(
                table: "RehabProgramExercises",
                keyColumn: "RehabProgramExerciseId",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "RehabProgramExercises",
                keyColumn: "RehabProgramExerciseId",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "RehabProgramExercises",
                keyColumn: "RehabProgramExerciseId",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "RehabProgramExercises",
                keyColumn: "RehabProgramExerciseId",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "RehabProgramExercises",
                keyColumn: "RehabProgramExerciseId",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "RehabProgramExercises",
                keyColumn: "RehabProgramExerciseId",
                keyValue: 9);

            migrationBuilder.DeleteData(
                table: "RehabProgramExercises",
                keyColumn: "RehabProgramExerciseId",
                keyValue: 10);

            migrationBuilder.DeleteData(
                table: "RehabProgramExercises",
                keyColumn: "RehabProgramExerciseId",
                keyValue: 11);

            migrationBuilder.DeleteData(
                table: "RehabProgramExercises",
                keyColumn: "RehabProgramExerciseId",
                keyValue: 12);

            migrationBuilder.DeleteData(
                table: "VideoSubmissions",
                keyColumn: "VideoSubmissionId",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "VideoSubmissions",
                keyColumn: "VideoSubmissionId",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "VideoSubmissions",
                keyColumn: "VideoSubmissionId",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "Exercises",
                keyColumn: "ExerciseId",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "Exercises",
                keyColumn: "ExerciseId",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "Exercises",
                keyColumn: "ExerciseId",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "MessageThreads",
                keyColumn: "MessageThreadId",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "MessageThreads",
                keyColumn: "MessageThreadId",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "MessageThreads",
                keyColumn: "MessageThreadId",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "MessageThreads",
                keyColumn: "MessageThreadId",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "MessageThreads",
                keyColumn: "MessageThreadId",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "Pets",
                keyColumn: "PetId",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "RehabPrograms",
                keyColumn: "RehabProgramId",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "RehabPrograms",
                keyColumn: "RehabProgramId",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "RehabPrograms",
                keyColumn: "RehabProgramId",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "RehabPrograms",
                keyColumn: "RehabProgramId",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "VideoSubmissions",
                keyColumn: "VideoSubmissionId",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "VideoSubmissions",
                keyColumn: "VideoSubmissionId",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "VideoSubmissions",
                keyColumn: "VideoSubmissionId",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "Exercises",
                keyColumn: "ExerciseId",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Exercises",
                keyColumn: "ExerciseId",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "Pets",
                keyColumn: "PetId",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Pets",
                keyColumn: "PetId",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Pets",
                keyColumn: "PetId",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Pets",
                keyColumn: "PetId",
                keyValue: 5);

            migrationBuilder.UpdateData(
                table: "Appointments",
                keyColumn: "AppointmentId",
                keyValue: 1,
                column: "ScheduledDateTime",
                value: new DateTime(2026, 7, 23, 10, 0, 0, 0, DateTimeKind.Utc));

            migrationBuilder.UpdateData(
                table: "DailyTrackingLogs",
                keyColumn: "LogId",
                keyValue: 1,
                column: "LogDate",
                value: new DateOnly(2026, 7, 17));

            migrationBuilder.UpdateData(
                table: "DailyTrackingLogs",
                keyColumn: "LogId",
                keyValue: 2,
                columns: new[] { "LogDate", "MobilityScore", "PainScore" },
                values: new object[] { new DateOnly(2026, 7, 18), 5, 6 });

            migrationBuilder.UpdateData(
                table: "DailyTrackingLogs",
                keyColumn: "LogId",
                keyValue: 3,
                columns: new[] { "EnergyScore", "LogDate", "MobilityScore" },
                values: new object[] { 6, new DateOnly(2026, 7, 19), 5 });

            migrationBuilder.UpdateData(
                table: "DailyTrackingLogs",
                keyColumn: "LogId",
                keyValue: 4,
                columns: new[] { "EnergyScore", "LogDate", "MobilityScore", "PainScore" },
                values: new object[] { 6, new DateOnly(2026, 7, 20), 6, 5 });

            migrationBuilder.UpdateData(
                table: "DailyTrackingLogs",
                keyColumn: "LogId",
                keyValue: 5,
                columns: new[] { "EnergyScore", "LogDate", "MobilityScore", "PainScore" },
                values: new object[] { 7, new DateOnly(2026, 7, 21), 6, 5 });

            migrationBuilder.UpdateData(
                table: "DailyTrackingLogs",
                keyColumn: "LogId",
                keyValue: 6,
                columns: new[] { "EnergyScore", "LogDate", "MobilityScore", "PainScore" },
                values: new object[] { 7, new DateOnly(2026, 7, 22), 7, 4 });

            migrationBuilder.UpdateData(
                table: "DailyTrackingLogs",
                keyColumn: "LogId",
                keyValue: 7,
                columns: new[] { "EnergyScore", "LogDate", "MobilityScore", "PainScore" },
                values: new object[] { 8, new DateOnly(2026, 7, 23), 7, 4 });

            migrationBuilder.UpdateData(
                table: "RehabPrograms",
                keyColumn: "RehabProgramId",
                keyValue: 1,
                column: "ProgramTitle",
                value: "Buddy Hip Recovery - Week 1");
        }
    }
}
