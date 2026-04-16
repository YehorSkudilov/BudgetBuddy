using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BudgetBuddyAPI.Migrations
{
    /// <inheritdoc />
    public partial class swddfdeddsdfsddf : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_BankAccounts_BankConnectionId",
                table: "BankAccounts");

            migrationBuilder.AddColumn<string>(
                name: "InstitutionId",
                table: "BankConnections",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "BankConnections",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_PlaidTransactionId",
                table: "Transactions",
                column: "PlaidTransactionId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BankConnections_UserId_InstitutionId",
                table: "BankConnections",
                columns: new[] { "UserId", "InstitutionId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BankAccounts_BankConnectionId_PlaidAccountId",
                table: "BankAccounts",
                columns: new[] { "BankConnectionId", "PlaidAccountId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Transactions_PlaidTransactionId",
                table: "Transactions");

            migrationBuilder.DropIndex(
                name: "IX_BankConnections_UserId_InstitutionId",
                table: "BankConnections");

            migrationBuilder.DropIndex(
                name: "IX_BankAccounts_BankConnectionId_PlaidAccountId",
                table: "BankAccounts");

            migrationBuilder.DropColumn(
                name: "InstitutionId",
                table: "BankConnections");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "BankConnections");

            migrationBuilder.CreateIndex(
                name: "IX_BankAccounts_BankConnectionId",
                table: "BankAccounts",
                column: "BankConnectionId");
        }
    }
}
