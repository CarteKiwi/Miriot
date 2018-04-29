using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace Miriot.Api.Migrations
{
    public partial class MiriotConfiguration : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Title",
                table: "Widgets");

            migrationBuilder.RenameColumn(
                name: "DeviceIdentifier",
                table: "Configurations",
                newName: "MiriotDeviceId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "MiriotDeviceId",
                table: "Configurations",
                newName: "DeviceIdentifier");

            migrationBuilder.AddColumn<string>(
                name: "Title",
                table: "Widgets",
                nullable: true);
        }
    }
}
