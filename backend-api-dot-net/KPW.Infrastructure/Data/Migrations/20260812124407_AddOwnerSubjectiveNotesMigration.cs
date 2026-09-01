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
            migrationBuilder.Sql(@"
                IF OBJECT_ID(N'[OwnerSubjectiveNotes]', N'U') IS NULL
                BEGIN
                    CREATE TABLE [OwnerSubjectiveNotes] (
                        [OwnerSubjectiveNoteId] int NOT NULL IDENTITY,
                        [PetId] int NOT NULL,
                        [OwnerId] int NOT NULL,
                        [NoteDate] datetime2 NOT NULL,
                        [Notes] nvarchar(2000) NOT NULL,
                        [PainObserved] int NULL,
                        [EnergyObserved] int NULL,
                        [IsReviewed] bit NOT NULL,
                        [CreatedDate] datetime2 NOT NULL,
                        [CreatedUserId] int NULL,
                        [ModifiedDate] datetime2 NOT NULL,
                        [ModifiedUserId] int NULL,
                        [IsActive] bit NOT NULL,
                        CONSTRAINT [PK_OwnerSubjectiveNotes] PRIMARY KEY ([OwnerSubjectiveNoteId]),
                        CONSTRAINT [FK_OwnerSubjectiveNotes_Pets_PetId] FOREIGN KEY ([PetId]) REFERENCES [Pets] ([PetId]) ON DELETE CASCADE,
                        CONSTRAINT [FK_OwnerSubjectiveNotes_Users_OwnerId] FOREIGN KEY ([OwnerId]) REFERENCES [Users] ([UserId]) ON DELETE NO ACTION
                    );
                    CREATE INDEX [IX_OwnerSubjectiveNotes_OwnerId] ON [OwnerSubjectiveNotes] ([OwnerId]);
                    CREATE INDEX [IX_OwnerSubjectiveNotes_PetId] ON [OwnerSubjectiveNotes] ([PetId]);
                END
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OwnerSubjectiveNotes");
        }
    }
}
