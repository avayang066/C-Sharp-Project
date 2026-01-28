using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyApp.Migrations
{
    /// <inheritdoc />
    public partial class SetAlarmEventProductionLogCascade : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AlarmEvents_ProductionLogs_ProductionLogId",
                table: "AlarmEvents");

            migrationBuilder.AddForeignKey(
                name: "FK_AlarmEvents_ProductionLogs_ProductionLogId",
                table: "AlarmEvents",
                column: "ProductionLogId",
                principalTable: "ProductionLogs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AlarmEvents_ProductionLogs_ProductionLogId",
                table: "AlarmEvents");

            migrationBuilder.AddForeignKey(
                name: "FK_AlarmEvents_ProductionLogs_ProductionLogId",
                table: "AlarmEvents",
                column: "ProductionLogId",
                principalTable: "ProductionLogs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
