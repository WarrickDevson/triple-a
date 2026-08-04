using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KPW.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class Phase5VideoProcessingStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ProcessingStatus",
                table: "VideoSubmissions",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "Pending");

            migrationBuilder.UpdateData(
                table: "VideoSubmissions",
                keyColumn: "VideoSubmissionId",
                keyValue: 1,
                columns: new[] { "ProcessingStatus", "RawVideoStorageUrl" },
                values: new object[] { "Pending", "videos/demo-buddy-sit-to-stand-raw.mp4" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ProcessingStatus",
                table: "VideoSubmissions");

            migrationBuilder.UpdateData(
                table: "VideoSubmissions",
                keyColumn: "VideoSubmissionId",
                keyValue: 1,
                column: "RawVideoStorageUrl",
                value: "gs://kpw-demo/buddy-sit-to-stand-raw.mp4");
        }
    }
}
