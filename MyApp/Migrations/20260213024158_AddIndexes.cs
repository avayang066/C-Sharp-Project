using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyApp.Migrations
{
    /// <inheritdoc />
    public partial class AddIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AlarmEvents_Machines_MachineId",
                table: "AlarmEvents"
            );

            migrationBuilder.DropForeignKey(
                name: "FK_AlarmEvents_ProductionLogs_ProductionLogId",
                table: "AlarmEvents"
            );

            migrationBuilder.DropForeignKey(
                name: "FK_ProductionLogs_Machines_MachineId",
                table: "ProductionLogs"
            );

            migrationBuilder.CreateIndex(
                name: "IX_Machines_IsActive",
                table: "Machines",
                column: "IsActive"
            );

            migrationBuilder.CreateIndex(
                name: "IX_Machines_MachineCode",
                table: "Machines",
                column: "MachineCode"
            );

            migrationBuilder.CreateIndex(
                name: "IX_AlarmEvents_CreatedAt",
                table: "AlarmEvents",
                column: "CreatedAt"
            );

            migrationBuilder.AddForeignKey(
                name: "FK_AlarmEvents_Machines_MachineId",
                table: "AlarmEvents",
                column: "MachineId",
                principalTable: "Machines",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict
            );

            migrationBuilder.AddForeignKey(
                name: "FK_AlarmEvents_ProductionLogs_ProductionLogId",
                table: "AlarmEvents",
                column: "ProductionLogId",
                principalTable: "ProductionLogs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict
            );

            migrationBuilder.AddForeignKey(
                name: "FK_ProductionLogs_Machines_MachineId",
                table: "ProductionLogs",
                column: "MachineId",
                principalTable: "Machines",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AlarmEvents_Machines_MachineId",
                table: "AlarmEvents"
            );

            migrationBuilder.DropForeignKey(
                name: "FK_AlarmEvents_ProductionLogs_ProductionLogId",
                table: "AlarmEvents"
            );

            migrationBuilder.DropForeignKey(
                name: "FK_ProductionLogs_Machines_MachineId",
                table: "ProductionLogs"
            );

            migrationBuilder.DropIndex(name: "IX_Machines_IsActive", table: "Machines");

            migrationBuilder.DropIndex(name: "IX_Machines_MachineCode", table: "Machines");

            migrationBuilder.DropIndex(name: "IX_AlarmEvents_CreatedAt", table: "AlarmEvents");

            migrationBuilder.AddForeignKey(
                name: "FK_AlarmEvents_Machines_MachineId",
                table: "AlarmEvents",
                column: "MachineId",
                principalTable: "Machines",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade
            );

            migrationBuilder.AddForeignKey(
                name: "FK_AlarmEvents_ProductionLogs_ProductionLogId",
                table: "AlarmEvents",
                column: "ProductionLogId",
                principalTable: "ProductionLogs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade
            );

            migrationBuilder.AddForeignKey(
                name: "FK_ProductionLogs_Machines_MachineId",
                table: "ProductionLogs",
                column: "MachineId",
                principalTable: "Machines",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade
            );
        }
    }
}
