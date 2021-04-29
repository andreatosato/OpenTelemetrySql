using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace SampleDatabase.Migrations
{
    public partial class Second : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Posts",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreateDate",
                value: new DateTime(2021, 4, 29, 21, 6, 59, 284, DateTimeKind.Local).AddTicks(595));

            migrationBuilder.UpdateData(
                table: "Posts",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreateDate",
                value: new DateTime(2021, 4, 29, 21, 56, 59, 287, DateTimeKind.Local).AddTicks(849));

            migrationBuilder.InsertData(
                table: "Posts",
                columns: new[] { "Id", "CreateDate", "Text" },
                values: new object[] { 3, new DateTime(2021, 4, 29, 20, 16, 59, 287, DateTimeKind.Local).AddTicks(903), "My Text 3" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Posts",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.UpdateData(
                table: "Posts",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreateDate",
                value: new DateTime(2021, 4, 29, 21, 2, 46, 860, DateTimeKind.Local).AddTicks(7016));

            migrationBuilder.UpdateData(
                table: "Posts",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreateDate",
                value: new DateTime(2021, 4, 29, 21, 52, 46, 863, DateTimeKind.Local).AddTicks(7179));
        }
    }
}
