using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VirtualCard.Migrations
{
    public partial class MakeAliasNullable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BlockedCards",
                columns: table => new
                {
                    id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    accountNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    cardReference = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BlockedCards", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "ChangePinRequests",
                columns: table => new
                {
                    id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    cardReference = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    accountNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    oldPin = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    newPin = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChangePinRequests", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "CreatedCards",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    alias = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    clientReference = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    cardReference = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    accountNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    pan = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    seqNr = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    expiryDate = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    pinOffset = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    cvv = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    cvv2 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    pinInfo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    track2 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    customerId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    defaultAccountType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    blocked = table.Column<bool>(type: "bit", nullable: true),
                    failedPinAttempts = table.Column<int>(type: "int", nullable: true),
                    creationChannel = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CreatedCards", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ResetPinResponses",
                columns: table => new
                {
                    id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    accountNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    cardReference = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ResetPinResponses", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "UnBlockedCards",
                columns: table => new
                {
                    id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    accountNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    cardReference = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UnBlockedCards", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "VirtualCardTransactionDisputes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AccountNumber = table.Column<long>(type: "bigint", nullable: false),
                    TransactionId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Subject = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    From = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    To = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Message = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Bcc = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Cc = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VirtualCardTransactionDisputes", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BlockedCards");

            migrationBuilder.DropTable(
                name: "ChangePinRequests");

            migrationBuilder.DropTable(
                name: "CreatedCards");

            migrationBuilder.DropTable(
                name: "ResetPinResponses");

            migrationBuilder.DropTable(
                name: "UnBlockedCards");

            migrationBuilder.DropTable(
                name: "VirtualCardTransactionDisputes");
        }
    }
}
