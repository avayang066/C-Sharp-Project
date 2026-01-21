using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyApp.Migrations
{
    /// <inheritdoc />
    public partial class AddAlarmEventFullSpec : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AlarmType",
                table: "AlarmEvents",
                type: "TEXT",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "MachineId",
                table: "AlarmEvents",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Message",
                table: "AlarmEvents",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<long>(
                name: "ProductionLogId",
                table: "AlarmEvents",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.CreateIndex(
                name: "IX_AlarmEvents_MachineId",
                table: "AlarmEvents",
                column: "MachineId");

            migrationBuilder.CreateIndex(
                name: "IX_AlarmEvents_ProductionLogId",
                table: "AlarmEvents",
                column: "ProductionLogId");

            migrationBuilder.AddForeignKey(
                name: "FK_AlarmEvents_Machines_MachineId",
                table: "AlarmEvents",
                column: "MachineId",
                principalTable: "Machines",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

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
                name: "FK_AlarmEvents_Machines_MachineId",
                table: "AlarmEvents");

            migrationBuilder.DropForeignKey(
                name: "FK_AlarmEvents_ProductionLogs_ProductionLogId",
                table: "AlarmEvents");

            migrationBuilder.DropIndex(
                name: "IX_AlarmEvents_MachineId",
                table: "AlarmEvents");

            migrationBuilder.DropIndex(
                name: "IX_AlarmEvents_ProductionLogId",
                table: "AlarmEvents");

            migrationBuilder.DropColumn(
                name: "AlarmType",
                table: "AlarmEvents");

            migrationBuilder.DropColumn(
                name: "MachineId",
                table: "AlarmEvents");

            migrationBuilder.DropColumn(
                name: "Message",
                table: "AlarmEvents");

            migrationBuilder.DropColumn(
                name: "ProductionLogId",
                table: "AlarmEvents");
        }
    }
}
