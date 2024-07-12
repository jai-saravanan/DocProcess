using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DocumentProcessorDB.Migrations
{
    /// <inheritdoc />
    public partial class addsource_destination_folder_info : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DestinationFolderName",
                table: "WorkerNode",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "SourceFolderName",
                table: "WorkerNode",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "DestinationFolderName",
                table: "TaskManager",
                type: "nvarchar(250)",
                maxLength: 250,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "SourceFolderName",
                table: "TaskManager",
                type: "nvarchar(250)",
                maxLength: 250,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DestinationFolderName",
                table: "WorkerNode");

            migrationBuilder.DropColumn(
                name: "SourceFolderName",
                table: "WorkerNode");

            migrationBuilder.DropColumn(
                name: "DestinationFolderName",
                table: "TaskManager");

            migrationBuilder.DropColumn(
                name: "SourceFolderName",
                table: "TaskManager");
        }
    }
}
