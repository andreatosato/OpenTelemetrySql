using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace SampleDatabase.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Blogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Blogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Posts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Text = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreateDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Posts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Surname = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PostUsers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PostId = table.Column<int>(type: "int", nullable: true),
                    UserId = table.Column<int>(type: "int", nullable: true),
                    BlogEntityId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PostUsers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PostUsers_Blogs_BlogEntityId",
                        column: x => x.BlogEntityId,
                        principalTable: "Blogs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PostUsers_Posts_PostId",
                        column: x => x.PostId,
                        principalTable: "Posts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PostUsers_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "Blogs",
                column: "Id",
                value: 1);

            migrationBuilder.InsertData(
                table: "Posts",
                columns: new[] { "Id", "CreateDate", "Text" },
                values: new object[,]
                {
                    { 1, new DateTime(2021, 4, 29, 21, 2, 46, 860, DateTimeKind.Local).AddTicks(7016), "My Text" },
                    { 2, new DateTime(2021, 4, 29, 21, 52, 46, 863, DateTimeKind.Local).AddTicks(7179), "My Text 2" }
                });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "Name", "Surname" },
                values: new object[,]
                {
                    { 1, "Andrea", "Tosato" },
                    { 2, "Mario", "Rossi" }
                });

            migrationBuilder.InsertData(
                table: "PostUsers",
                columns: new[] { "Id", "BlogEntityId", "PostId", "UserId" },
                values: new object[] { 1, 1, 1, 1 });

            migrationBuilder.InsertData(
                table: "PostUsers",
                columns: new[] { "Id", "BlogEntityId", "PostId", "UserId" },
                values: new object[] { 2, 1, 2, 2 });

            migrationBuilder.CreateIndex(
                name: "IX_PostUsers_BlogEntityId",
                table: "PostUsers",
                column: "BlogEntityId");

            migrationBuilder.CreateIndex(
                name: "IX_PostUsers_PostId",
                table: "PostUsers",
                column: "PostId");

            migrationBuilder.CreateIndex(
                name: "IX_PostUsers_UserId",
                table: "PostUsers",
                column: "UserId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PostUsers");

            migrationBuilder.DropTable(
                name: "Blogs");

            migrationBuilder.DropTable(
                name: "Posts");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
