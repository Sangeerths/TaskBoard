using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaskBoard.Migrations
{
    /// <inheritdoc />
    public partial class AddTaskStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "TaskItems",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "Started");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Status",
                table: "TaskItems");
        }
    }
}
