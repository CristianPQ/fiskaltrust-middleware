﻿using Microsoft.EntityFrameworkCore.Migrations;

namespace fiskaltrust.Middleware.Storage.EFCore.SQLServer.Migrations
{
    public partial class Indices : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "cbReceiptReference",
                table: "ftQueueItem",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ftReceiptJournal_TimeStamp",
                table: "ftReceiptJournal",
                column: "TimeStamp");

            migrationBuilder.CreateIndex(
                name: "IX_ftQueueItem_cbReceiptReference",
                table: "ftQueueItem",
                column: "cbReceiptReference");

            migrationBuilder.CreateIndex(
                name: "IX_ftQueueItem_TimeStamp",
                table: "ftQueueItem",
                column: "TimeStamp");

            migrationBuilder.CreateIndex(
                name: "IX_ftJournalFR_TimeStamp",
                table: "ftJournalFR",
                column: "TimeStamp");

            migrationBuilder.CreateIndex(
                name: "IX_ftJournalDE_TimeStamp",
                table: "ftJournalDE",
                column: "TimeStamp");

            migrationBuilder.CreateIndex(
                name: "IX_ftJournalAT_TimeStamp",
                table: "ftJournalAT",
                column: "TimeStamp");

            migrationBuilder.CreateIndex(
                name: "IX_ftActionJournal_TimeStamp",
                table: "ftActionJournal",
                column: "TimeStamp");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ftReceiptJournal_TimeStamp",
                table: "ftReceiptJournal");

            migrationBuilder.DropIndex(
                name: "IX_ftQueueItem_cbReceiptReference",
                table: "ftQueueItem");

            migrationBuilder.DropIndex(
                name: "IX_ftQueueItem_TimeStamp",
                table: "ftQueueItem");

            migrationBuilder.DropIndex(
                name: "IX_ftJournalFR_TimeStamp",
                table: "ftJournalFR");

            migrationBuilder.DropIndex(
                name: "IX_ftJournalDE_TimeStamp",
                table: "ftJournalDE");

            migrationBuilder.DropIndex(
                name: "IX_ftJournalAT_TimeStamp",
                table: "ftJournalAT");

            migrationBuilder.DropIndex(
                name: "IX_ftActionJournal_TimeStamp",
                table: "ftActionJournal");

            migrationBuilder.AlterColumn<string>(
                name: "cbReceiptReference",
                table: "ftQueueItem",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);
        }
    }
}
