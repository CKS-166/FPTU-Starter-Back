using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FPTU_Starter.Infrastructure.Migrations
{
    public partial class miggrationsnull : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Transactions_SystemWallets_SystemWalletId",
                table: "Transactions");

            migrationBuilder.AlterColumn<Guid>(
                name: "SystemWalletId",
                table: "Transactions",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AddForeignKey(
                name: "FK_Transactions_SystemWallets_SystemWalletId",
                table: "Transactions",
                column: "SystemWalletId",
                principalTable: "SystemWallets",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Transactions_SystemWallets_SystemWalletId",
                table: "Transactions");

            migrationBuilder.AlterColumn<Guid>(
                name: "SystemWalletId",
                table: "Transactions",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Transactions_SystemWallets_SystemWalletId",
                table: "Transactions",
                column: "SystemWalletId",
                principalTable: "SystemWallets",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
