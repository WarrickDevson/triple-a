using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KPW.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddMessageAttachments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AttachmentName",
                table: "Messages",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AttachmentType",
                table: "Messages",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AttachmentUrl",
                table: "Messages",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Messages",
                keyColumn: "MessageId",
                keyValue: 1,
                columns: new[] { "AttachmentName", "AttachmentType", "AttachmentUrl" },
                values: new object[] { null, null, null });

            migrationBuilder.UpdateData(
                table: "Messages",
                keyColumn: "MessageId",
                keyValue: 2,
                columns: new[] { "AttachmentName", "AttachmentType", "AttachmentUrl" },
                values: new object[] { null, null, null });

            migrationBuilder.UpdateData(
                table: "Messages",
                keyColumn: "MessageId",
                keyValue: 3,
                columns: new[] { "AttachmentName", "AttachmentType", "AttachmentUrl" },
                values: new object[] { null, null, null });

            migrationBuilder.UpdateData(
                table: "Messages",
                keyColumn: "MessageId",
                keyValue: 4,
                columns: new[] { "AttachmentName", "AttachmentType", "AttachmentUrl" },
                values: new object[] { null, null, null });

            migrationBuilder.UpdateData(
                table: "Messages",
                keyColumn: "MessageId",
                keyValue: 5,
                columns: new[] { "AttachmentName", "AttachmentType", "AttachmentUrl" },
                values: new object[] { null, null, null });

            migrationBuilder.UpdateData(
                table: "Messages",
                keyColumn: "MessageId",
                keyValue: 6,
                columns: new[] { "AttachmentName", "AttachmentType", "AttachmentUrl" },
                values: new object[] { null, null, null });

            migrationBuilder.UpdateData(
                table: "Messages",
                keyColumn: "MessageId",
                keyValue: 7,
                columns: new[] { "AttachmentName", "AttachmentType", "AttachmentUrl" },
                values: new object[] { null, null, null });

            migrationBuilder.UpdateData(
                table: "Messages",
                keyColumn: "MessageId",
                keyValue: 8,
                columns: new[] { "AttachmentName", "AttachmentType", "AttachmentUrl" },
                values: new object[] { null, null, null });

            migrationBuilder.UpdateData(
                table: "Messages",
                keyColumn: "MessageId",
                keyValue: 9,
                columns: new[] { "AttachmentName", "AttachmentType", "AttachmentUrl" },
                values: new object[] { null, null, null });

            migrationBuilder.UpdateData(
                table: "Messages",
                keyColumn: "MessageId",
                keyValue: 10,
                columns: new[] { "AttachmentName", "AttachmentType", "AttachmentUrl" },
                values: new object[] { null, null, null });

            migrationBuilder.UpdateData(
                table: "Messages",
                keyColumn: "MessageId",
                keyValue: 11,
                columns: new[] { "AttachmentName", "AttachmentType", "AttachmentUrl" },
                values: new object[] { null, null, null });

            migrationBuilder.UpdateData(
                table: "Messages",
                keyColumn: "MessageId",
                keyValue: 12,
                columns: new[] { "AttachmentName", "AttachmentType", "AttachmentUrl" },
                values: new object[] { null, null, null });

            migrationBuilder.UpdateData(
                table: "Messages",
                keyColumn: "MessageId",
                keyValue: 13,
                columns: new[] { "AttachmentName", "AttachmentType", "AttachmentUrl" },
                values: new object[] { null, null, null });

            migrationBuilder.UpdateData(
                table: "Messages",
                keyColumn: "MessageId",
                keyValue: 14,
                columns: new[] { "AttachmentName", "AttachmentType", "AttachmentUrl" },
                values: new object[] { null, null, null });

            migrationBuilder.UpdateData(
                table: "Messages",
                keyColumn: "MessageId",
                keyValue: 15,
                columns: new[] { "AttachmentName", "AttachmentType", "AttachmentUrl" },
                values: new object[] { null, null, null });

            migrationBuilder.UpdateData(
                table: "Messages",
                keyColumn: "MessageId",
                keyValue: 16,
                columns: new[] { "AttachmentName", "AttachmentType", "AttachmentUrl" },
                values: new object[] { null, null, null });

            migrationBuilder.UpdateData(
                table: "Messages",
                keyColumn: "MessageId",
                keyValue: 17,
                columns: new[] { "AttachmentName", "AttachmentType", "AttachmentUrl" },
                values: new object[] { null, null, null });

            migrationBuilder.UpdateData(
                table: "Messages",
                keyColumn: "MessageId",
                keyValue: 18,
                columns: new[] { "AttachmentName", "AttachmentType", "AttachmentUrl" },
                values: new object[] { null, null, null });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 1,
                column: "IsEmailVerified",
                value: true);

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 2,
                column: "IsEmailVerified",
                value: true);

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 3,
                column: "IsEmailVerified",
                value: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AttachmentName",
                table: "Messages");

            migrationBuilder.DropColumn(
                name: "AttachmentType",
                table: "Messages");

            migrationBuilder.DropColumn(
                name: "AttachmentUrl",
                table: "Messages");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 1,
                column: "IsEmailVerified",
                value: false);

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 2,
                column: "IsEmailVerified",
                value: false);

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 3,
                column: "IsEmailVerified",
                value: false);
        }
    }
}
