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

            migrationBuilder.Sql(@"
                IF OBJECT_ID(N'[SoapNotes]', N'U') IS NULL
                BEGIN
                    CREATE TABLE [SoapNotes] (
                        [SoapNoteId] int NOT NULL IDENTITY,
                        [PetId] int NOT NULL,
                        [PhysioId] int NOT NULL,
                        [AppointmentId] int NULL,
                        [SessionDate] datetime2 NOT NULL,
                        [Subjective] nvarchar(4000) NOT NULL,
                        [Objective] nvarchar(4000) NOT NULL,
                        [Action] nvarchar(4000) NOT NULL,
                        [Plan] nvarchar(4000) NOT NULL,
                        [StiffnessScore] int NULL,
                        [PainScore] int NULL,
                        [LamenessScore] int NULL,
                        [CustomMetricsJson] nvarchar(max) NULL,
                        [IsSharedWithOwner] bit NOT NULL,
                        [SharedAtUtc] datetime2 NULL,
                        [CreatedDate] datetime2 NOT NULL,
                        [CreatedUserId] int NULL,
                        [ModifiedDate] datetime2 NOT NULL,
                        [ModifiedUserId] int NULL,
                        [IsActive] bit NOT NULL,
                        CONSTRAINT [PK_SoapNotes] PRIMARY KEY ([SoapNoteId]),
                        CONSTRAINT [FK_SoapNotes_Appointments_AppointmentId] FOREIGN KEY ([AppointmentId]) REFERENCES [Appointments] ([AppointmentId]) ON DELETE SET NULL,
                        CONSTRAINT [FK_SoapNotes_Pets_PetId] FOREIGN KEY ([PetId]) REFERENCES [Pets] ([PetId]) ON DELETE CASCADE,
                        CONSTRAINT [FK_SoapNotes_Users_PhysioId] FOREIGN KEY ([PhysioId]) REFERENCES [Users] ([UserId]) ON DELETE NO ACTION
                    );
                    CREATE INDEX [IX_SoapNotes_AppointmentId] ON [SoapNotes] ([AppointmentId]);
                    CREATE INDEX [IX_SoapNotes_PetId] ON [SoapNotes] ([PetId]);
                    CREATE INDEX [IX_SoapNotes_PhysioId] ON [SoapNotes] ([PhysioId]);
                END

                IF OBJECT_ID(N'[SharedReports]', N'U') IS NULL
                BEGIN
                    CREATE TABLE [SharedReports] (
                        [SharedReportId] int NOT NULL IDENTITY,
                        [PetId] int NOT NULL,
                        [SoapNoteId] int NULL,
                        [SharedByPhysioId] int NOT NULL,
                        [Title] nvarchar(200) NOT NULL,
                        [ReportType] nvarchar(50) NOT NULL,
                        [Summary] nvarchar(1000) NULL,
                        [SharedAtUtc] datetime2 NOT NULL,
                        [CreatedDate] datetime2 NOT NULL,
                        [CreatedUserId] int NULL,
                        [ModifiedDate] datetime2 NOT NULL,
                        [ModifiedUserId] int NULL,
                        [IsActive] bit NOT NULL,
                        CONSTRAINT [PK_SharedReports] PRIMARY KEY ([SharedReportId]),
                        CONSTRAINT [FK_SharedReports_Pets_PetId] FOREIGN KEY ([PetId]) REFERENCES [Pets] ([PetId]) ON DELETE CASCADE,
                        CONSTRAINT [FK_SharedReports_SoapNotes_SoapNoteId] FOREIGN KEY ([SoapNoteId]) REFERENCES [SoapNotes] ([SoapNoteId]) ON DELETE SET NULL,
                        CONSTRAINT [FK_SharedReports_Users_SharedByPhysioId] FOREIGN KEY ([SharedByPhysioId]) REFERENCES [Users] ([UserId]) ON DELETE NO ACTION
                    );
                    CREATE INDEX [IX_SharedReports_PetId] ON [SharedReports] ([PetId]);
                    CREATE INDEX [IX_SharedReports_SharedByPhysioId] ON [SharedReports] ([SharedByPhysioId]);
                    CREATE INDEX [IX_SharedReports_SoapNoteId] ON [SharedReports] ([SoapNoteId]);
                END
            ");

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
