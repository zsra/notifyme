using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NotifyMe.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddUsersAndOwnership : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "OwnerUserId",
                table: "subscriptions",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "OwnerUserId",
                table: "channel_configs",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "OwnerUserId",
                table: "alert_rules",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Email = table.Column<string>(type: "character varying(320)", maxLength: 320, nullable: false),
                    PasswordHash = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_subscriptions_OwnerUserId",
                table: "subscriptions",
                column: "OwnerUserId");

            migrationBuilder.CreateIndex(
                name: "IX_channel_configs_OwnerUserId",
                table: "channel_configs",
                column: "OwnerUserId");

            migrationBuilder.CreateIndex(
                name: "IX_alert_rules_OwnerUserId",
                table: "alert_rules",
                column: "OwnerUserId");

            migrationBuilder.CreateIndex(
                name: "IX_users_Email",
                table: "users",
                column: "Email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "users");

            migrationBuilder.DropIndex(
                name: "IX_subscriptions_OwnerUserId",
                table: "subscriptions");

            migrationBuilder.DropIndex(
                name: "IX_channel_configs_OwnerUserId",
                table: "channel_configs");

            migrationBuilder.DropIndex(
                name: "IX_alert_rules_OwnerUserId",
                table: "alert_rules");

            migrationBuilder.DropColumn(
                name: "OwnerUserId",
                table: "subscriptions");

            migrationBuilder.DropColumn(
                name: "OwnerUserId",
                table: "channel_configs");

            migrationBuilder.DropColumn(
                name: "OwnerUserId",
                table: "alert_rules");
        }
    }
}
