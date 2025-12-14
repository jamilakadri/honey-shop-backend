using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MielShop.API.Migrations
{
    /// <inheritdoc />
    public partial class AddEmailVerificationFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "emailverificationtoken",
                table: "users",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "emailverificationtokenexpires",
                table: "users",
                type: "timestamp without time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "emailverificationtoken",
                table: "users");

            migrationBuilder.DropColumn(
                name: "emailverificationtokenexpires",
                table: "users");
        }
    }
}
