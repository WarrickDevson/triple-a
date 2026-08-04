using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace KPW.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class Phase4DashboardSeed : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Appointments",
                columns: new[] { "AppointmentId", "AppointmentStatus", "ClientNotes", "ClinicianNotes", "CreatedDate", "CreatedUserId", "IsActive", "ModifiedDate", "ModifiedUserId", "OwnerId", "PetId", "PhysioId", "ScheduledDateTime" },
                values: new object[] { 1, "Scheduled", "Follow-up on hip mobility progress.", null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 3, 1, 2, new DateTime(2026, 7, 23, 10, 0, 0, 0, DateTimeKind.Utc) });

            migrationBuilder.InsertData(
                table: "DailyTrackingLogs",
                columns: new[] { "LogId", "AppetiteScore", "CreatedDate", "CreatedUserId", "EnergyScore", "IsActive", "IsCompleted", "LamenessScore", "LogDate", "MobilityScore", "ModifiedDate", "ModifiedUserId", "PainScore", "PetId", "WeightKg" },
                values: new object[,]
                {
                    { 1, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 5, true, true, null, new DateOnly(2026, 7, 17), 4, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 7, 1, null },
                    { 2, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 5, true, true, null, new DateOnly(2026, 7, 18), 5, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 6, 1, null },
                    { 3, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 6, true, true, null, new DateOnly(2026, 7, 19), 5, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 6, 1, null },
                    { 4, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 6, true, true, null, new DateOnly(2026, 7, 20), 6, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 5, 1, null },
                    { 5, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 7, true, true, null, new DateOnly(2026, 7, 21), 6, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 5, 1, null },
                    { 6, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 7, true, true, null, new DateOnly(2026, 7, 22), 7, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 4, 1, null },
                    { 7, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 8, true, true, null, new DateOnly(2026, 7, 23), 7, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 4, 1, null }
                });

            migrationBuilder.InsertData(
                table: "VideoSubmissions",
                columns: new[] { "VideoSubmissionId", "CreatedDate", "CreatedUserId", "ExerciseId", "IsActive", "IsReviewed", "ModifiedDate", "ModifiedUserId", "PetId", "PhysioFeedbackNotes", "ProcessedVideoStreamingUrl", "RawVideoStorageUrl" },
                values: new object[] { 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 1, true, false, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 1, null, null, "gs://kpw-demo/buddy-sit-to-stand-raw.mp4" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Appointments",
                keyColumn: "AppointmentId",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "DailyTrackingLogs",
                keyColumn: "LogId",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "DailyTrackingLogs",
                keyColumn: "LogId",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "DailyTrackingLogs",
                keyColumn: "LogId",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "DailyTrackingLogs",
                keyColumn: "LogId",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "DailyTrackingLogs",
                keyColumn: "LogId",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "DailyTrackingLogs",
                keyColumn: "LogId",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "DailyTrackingLogs",
                keyColumn: "LogId",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "VideoSubmissions",
                keyColumn: "VideoSubmissionId",
                keyValue: 1);
        }
    }
}
