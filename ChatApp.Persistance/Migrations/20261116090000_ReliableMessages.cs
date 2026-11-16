using ChatApp.Persistance.Contexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace ChatApp.Persistance.Migrations;

[DbContext(typeof(DatabaseContext))]
[Migration("20261116090000_ReliableMessages")]
public class ReliableMessages : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // Do not silently rewrite sequence-based cursors on an existing installation.
        migrationBuilder.Sql(@"
            IF EXISTS (SELECT 1 FROM Messages GROUP BY ConversationId, SequenceNumber HAVING COUNT(*) > 1)
                THROW 51001, 'Duplicate message sequences exist. Back up and repair them before migrating.', 1;");
        migrationBuilder.AddColumn<Guid>(name: "ClientRequestId", table: "Messages",
            type: "uniqueidentifier", nullable: true);
        migrationBuilder.CreateIndex(name: "IX_Messages_ConversationId_SequenceNumber", table: "Messages",
            columns: new[] { "ConversationId", "SequenceNumber" }, unique: true);
        migrationBuilder.CreateIndex(name: "IX_Messages_SenderId_ClientRequestId", table: "Messages",
            columns: new[] { "SenderId", "ClientRequestId" }, unique: true,
            filter: "[ClientRequestId] IS NOT NULL");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex("IX_Messages_ConversationId_SequenceNumber", "Messages");
        migrationBuilder.DropIndex("IX_Messages_SenderId_ClientRequestId", "Messages");
        migrationBuilder.DropColumn("ClientRequestId", "Messages");
    }

    protected override void BuildTargetModel(ModelBuilder modelBuilder) =>
        ReliableMessagesModel.Build(modelBuilder);
}
