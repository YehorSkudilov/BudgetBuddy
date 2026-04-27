using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BudgetBuddyAPI.Migrations
{
    /// <inheritdoc />
    public partial class swdssdfedsdfsddf : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Institutions",
                columns: table => new
                {
                    id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    institution_id = table.Column<string>(type: "TEXT", nullable: false),
                    name = table.Column<string>(type: "TEXT", nullable: false),
                    country_codes = table.Column<string>(type: "TEXT", nullable: false),
                    products = table.Column<string>(type: "TEXT", nullable: false),
                    updated_at = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Institutions", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "BankConnections",
                columns: table => new
                {
                    id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    user_id = table.Column<Guid>(type: "TEXT", nullable: false),
                    item_id = table.Column<string>(type: "TEXT", nullable: false),
                    access_token = table.Column<string>(type: "TEXT", nullable: false),
                    institution_id = table.Column<int>(type: "INTEGER", nullable: false),
                    transactions_cursor = table.Column<string>(type: "TEXT", nullable: true),
                    created_at = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BankConnections", x => x.id);
                    table.ForeignKey(
                        name: "FK_BankConnections_Institutions_institution_id",
                        column: x => x.institution_id,
                        principalTable: "Institutions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "BankAccounts",
                columns: table => new
                {
                    id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    account_id = table.Column<string>(type: "TEXT", nullable: false),
                    mask = table.Column<string>(type: "TEXT", nullable: true),
                    name = table.Column<string>(type: "TEXT", nullable: false),
                    official_name = table.Column<string>(type: "TEXT", nullable: true),
                    type = table.Column<string>(type: "TEXT", nullable: false),
                    subtype = table.Column<string>(type: "TEXT", nullable: false),
                    holder_category = table.Column<string>(type: "TEXT", nullable: true),
                    balances_available = table.Column<decimal>(type: "TEXT", nullable: true),
                    balances_current = table.Column<decimal>(type: "TEXT", nullable: false),
                    balances_iso_currency_code = table.Column<string>(type: "TEXT", nullable: true),
                    balances_unofficial_currency_code = table.Column<string>(type: "TEXT", nullable: true),
                    balances_limit = table.Column<decimal>(type: "TEXT", nullable: true),
                    bank_connection_id = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BankAccounts", x => x.id);
                    table.ForeignKey(
                        name: "FK_BankAccounts_BankConnections_bank_connection_id",
                        column: x => x.bank_connection_id,
                        principalTable: "BankConnections",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Transactions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "TEXT", nullable: false),
                    transaction_id = table.Column<string>(type: "TEXT", nullable: false),
                    account_id = table.Column<string>(type: "TEXT", nullable: false),
                    amount = table.Column<decimal>(type: "TEXT", nullable: false),
                    name = table.Column<string>(type: "TEXT", nullable: false),
                    merchant_name = table.Column<string>(type: "TEXT", nullable: true),
                    merchant_entity_id = table.Column<string>(type: "TEXT", nullable: true),
                    iso_currency_code = table.Column<string>(type: "TEXT", nullable: true),
                    unofficial_currency_code = table.Column<string>(type: "TEXT", nullable: true),
                    date = table.Column<DateTime>(type: "TEXT", nullable: false),
                    authorized_date = table.Column<DateTime>(type: "TEXT", nullable: true),
                    datetime = table.Column<DateTime>(type: "TEXT", nullable: true),
                    authorized_datetime = table.Column<DateTime>(type: "TEXT", nullable: true),
                    pending = table.Column<bool>(type: "INTEGER", nullable: false),
                    pending_transaction_id = table.Column<string>(type: "TEXT", nullable: true),
                    payment_channel = table.Column<string>(type: "TEXT", nullable: true),
                    transaction_type = table.Column<string>(type: "TEXT", nullable: true),
                    transaction_code = table.Column<string>(type: "TEXT", nullable: true),
                    logo_url = table.Column<string>(type: "TEXT", nullable: true),
                    website = table.Column<string>(type: "TEXT", nullable: true),
                    personal_finance_category_icon_url = table.Column<string>(type: "TEXT", nullable: true),
                    account_owner = table.Column<string>(type: "TEXT", nullable: true),
                    check_number = table.Column<string>(type: "TEXT", nullable: true),
                    personal_finance_category_primary = table.Column<string>(type: "TEXT", nullable: true),
                    personal_finance_category_detailed = table.Column<string>(type: "TEXT", nullable: true),
                    personal_finance_category_confidence_level = table.Column<string>(type: "TEXT", nullable: true),
                    bank_account_id = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Transactions", x => x.id);
                    table.ForeignKey(
                        name: "FK_Transactions_BankAccounts_bank_account_id",
                        column: x => x.bank_account_id,
                        principalTable: "BankAccounts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TransactionCounterparty",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    name = table.Column<string>(type: "TEXT", nullable: true),
                    entity_id = table.Column<string>(type: "TEXT", nullable: true),
                    logo_url = table.Column<string>(type: "TEXT", nullable: true),
                    website = table.Column<string>(type: "TEXT", nullable: true),
                    type = table.Column<string>(type: "TEXT", nullable: true),
                    confidence_level = table.Column<string>(type: "TEXT", nullable: true),
                    phone_number = table.Column<string>(type: "TEXT", nullable: true),
                    TransactionId = table.Column<Guid>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TransactionCounterparty", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TransactionCounterparty_Transactions_TransactionId",
                        column: x => x.TransactionId,
                        principalTable: "Transactions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TransactionLocation",
                columns: table => new
                {
                    id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    address = table.Column<string>(type: "TEXT", nullable: true),
                    city = table.Column<string>(type: "TEXT", nullable: true),
                    region = table.Column<string>(type: "TEXT", nullable: true),
                    country = table.Column<string>(type: "TEXT", nullable: true),
                    postal_code = table.Column<string>(type: "TEXT", nullable: true),
                    lat = table.Column<double>(type: "REAL", nullable: true),
                    lon = table.Column<double>(type: "REAL", nullable: true),
                    store_number = table.Column<string>(type: "TEXT", nullable: true),
                    transaction_id = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TransactionLocation", x => x.id);
                    table.ForeignKey(
                        name: "FK_TransactionLocation_Transactions_transaction_id",
                        column: x => x.transaction_id,
                        principalTable: "Transactions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TransactionPaymentMeta",
                columns: table => new
                {
                    id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    by_order_of = table.Column<string>(type: "TEXT", nullable: true),
                    payee = table.Column<string>(type: "TEXT", nullable: true),
                    payer = table.Column<string>(type: "TEXT", nullable: true),
                    payment_method = table.Column<string>(type: "TEXT", nullable: true),
                    payment_processor = table.Column<string>(type: "TEXT", nullable: true),
                    ppd_id = table.Column<string>(type: "TEXT", nullable: true),
                    reason = table.Column<string>(type: "TEXT", nullable: true),
                    reference_number = table.Column<string>(type: "TEXT", nullable: true),
                    transaction_id = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TransactionPaymentMeta", x => x.id);
                    table.ForeignKey(
                        name: "FK_TransactionPaymentMeta_Transactions_transaction_id",
                        column: x => x.transaction_id,
                        principalTable: "Transactions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BankAccounts_bank_connection_id_account_id",
                table: "BankAccounts",
                columns: new[] { "bank_connection_id", "account_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BankConnections_institution_id",
                table: "BankConnections",
                column: "institution_id");

            migrationBuilder.CreateIndex(
                name: "IX_BankConnections_user_id_item_id",
                table: "BankConnections",
                columns: new[] { "user_id", "item_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Institutions_institution_id",
                table: "Institutions",
                column: "institution_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TransactionCounterparty_TransactionId",
                table: "TransactionCounterparty",
                column: "TransactionId");

            migrationBuilder.CreateIndex(
                name: "IX_TransactionLocation_transaction_id",
                table: "TransactionLocation",
                column: "transaction_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TransactionPaymentMeta_transaction_id",
                table: "TransactionPaymentMeta",
                column: "transaction_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_bank_account_id",
                table: "Transactions",
                column: "bank_account_id");

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_transaction_id",
                table: "Transactions",
                column: "transaction_id",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TransactionCounterparty");

            migrationBuilder.DropTable(
                name: "TransactionLocation");

            migrationBuilder.DropTable(
                name: "TransactionPaymentMeta");

            migrationBuilder.DropTable(
                name: "Transactions");

            migrationBuilder.DropTable(
                name: "BankAccounts");

            migrationBuilder.DropTable(
                name: "BankConnections");

            migrationBuilder.DropTable(
                name: "Institutions");
        }
    }
}
