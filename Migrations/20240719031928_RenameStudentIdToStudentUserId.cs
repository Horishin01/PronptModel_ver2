using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PronptModel_ver2.Migrations
{
    /// <inheritdoc />
    public partial class RenameStudentIdToStudentUserId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Days_AspNetUsers_StudentUserId",
                table: "Days");

            migrationBuilder.DropColumn(
                name: "StudentId",
                table: "Days");

            migrationBuilder.AlterColumn<string>(
                name: "StudentUserId",
                table: "Days",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Days_AspNetUsers_StudentUserId",
                table: "Days",
                column: "StudentUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Days_AspNetUsers_StudentUserId",
                table: "Days");

            migrationBuilder.AlterColumn<string>(
                name: "StudentUserId",
                table: "Days",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<string>(
                name: "StudentId",
                table: "Days",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddForeignKey(
                name: "FK_Days_AspNetUsers_StudentUserId",
                table: "Days",
                column: "StudentUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }
    }
}
