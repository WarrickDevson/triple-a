using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KPW.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddOwnerSubjectiveNotesMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "OwnerSubjectiveNotes",
                columns: table => new
                {
                    OwnerSubjectiveNoteId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PetId = table.Column<int>(type: "int", nullable: false),
                    OwnerId = table.Column<int>(type: "int", nullable: false),
                    NoteDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    PainObserved = table.Column<int>(type: "int", nullable: true),
                    EnergyObserved = table.Column<int>(type: "int", nullable: true),
                    IsReviewed = table.Column<bool>(type: "bit", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedUserId = table.Column<int>(type: "int", nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedUserId = table.Column<int>(type: "int", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OwnerSubjectiveNotes", x => x.OwnerSubjectiveNoteId);
                    table.ForeignKey(
                        name: "FK_OwnerSubjectiveNotes_Pets_PetId",
                        column: x => x.PetId,
                        principalTable: "Pets",
                        principalColumn: "PetId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OwnerSubjectiveNotes_Users_OwnerId",
                        column: x => x.OwnerId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_OwnerSubjectiveNotes_OwnerId",
                table: "OwnerSubjectiveNotes",
                column: "OwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_OwnerSubjectiveNotes_PetId",
                table: "OwnerSubjectiveNotes",
                column: "PetId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OwnerSubjectiveNotes");
        }
    }
}
