using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KPW.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddSoapNotesAndSharedReports : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Note: IsApproved was already added in database
            // migrationBuilder.AddColumn<bool>(
            //     name: "IsApproved",
            //     table: "Users",
            //     type: "bit",
            //     nullable: false,
            //     defaultValue: false);

            migrationBuilder.CreateTable(
                name: "SoapNotes",
                columns: table => new
                {
                    SoapNoteId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PetId = table.Column<int>(type: "int", nullable: false),
                    PhysioId = table.Column<int>(type: "int", nullable: false),
                    AppointmentId = table.Column<int>(type: "int", nullable: true),
                    SessionDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Subjective = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false),
                    Objective = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false),
                    Action = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false),
                    Plan = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false),
                    StiffnessScore = table.Column<int>(type: "int", nullable: true),
                    PainScore = table.Column<int>(type: "int", nullable: true),
                    LamenessScore = table.Column<int>(type: "int", nullable: true),
                    CustomMetricsJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsSharedWithOwner = table.Column<bool>(type: "bit", nullable: false),
                    SharedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedUserId = table.Column<int>(type: "int", nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedUserId = table.Column<int>(type: "int", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SoapNotes", x => x.SoapNoteId);
                    table.ForeignKey(
                        name: "FK_SoapNotes_Appointments_AppointmentId",
                        column: x => x.AppointmentId,
                        principalTable: "Appointments",
                        principalColumn: "AppointmentId",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_SoapNotes_Pets_PetId",
                        column: x => x.PetId,
                        principalTable: "Pets",
                        principalColumn: "PetId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SoapNotes_Users_PhysioId",
                        column: x => x.PhysioId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SharedReports",
                columns: table => new
                {
                    SharedReportId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PetId = table.Column<int>(type: "int", nullable: false),
                    SoapNoteId = table.Column<int>(type: "int", nullable: true),
                    SharedByPhysioId = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    ReportType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Summary = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    SharedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedUserId = table.Column<int>(type: "int", nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedUserId = table.Column<int>(type: "int", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SharedReports", x => x.SharedReportId);
                    table.ForeignKey(
                        name: "FK_SharedReports_Pets_PetId",
                        column: x => x.PetId,
                        principalTable: "Pets",
                        principalColumn: "PetId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SharedReports_SoapNotes_SoapNoteId",
                        column: x => x.SoapNoteId,
                        principalTable: "SoapNotes",
                        principalColumn: "SoapNoteId",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_SharedReports_Users_SharedByPhysioId",
                        column: x => x.SharedByPhysioId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 1,
                column: "IsApproved",
                value: true);

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 2,
                column: "IsApproved",
                value: true);

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 3,
                column: "IsApproved",
                value: true);

            migrationBuilder.CreateIndex(
                name: "IX_SharedReports_PetId",
                table: "SharedReports",
                column: "PetId");

            migrationBuilder.CreateIndex(
                name: "IX_SharedReports_SharedByPhysioId",
                table: "SharedReports",
                column: "SharedByPhysioId");

            migrationBuilder.CreateIndex(
                name: "IX_SharedReports_SoapNoteId",
                table: "SharedReports",
                column: "SoapNoteId");

            migrationBuilder.CreateIndex(
                name: "IX_SoapNotes_AppointmentId",
                table: "SoapNotes",
                column: "AppointmentId");

            migrationBuilder.CreateIndex(
                name: "IX_SoapNotes_PetId",
                table: "SoapNotes",
                column: "PetId");

            migrationBuilder.CreateIndex(
                name: "IX_SoapNotes_PhysioId",
                table: "SoapNotes",
                column: "PhysioId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SharedReports");

            migrationBuilder.DropTable(
                name: "SoapNotes");

            migrationBuilder.DropColumn(
                name: "IsApproved",
                table: "Users");
        }
    }
}
