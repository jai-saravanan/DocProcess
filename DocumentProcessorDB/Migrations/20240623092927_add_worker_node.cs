using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DocumentProcessorDB.Migrations
{
    /// <inheritdoc />
    public partial class add_worker_node : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "WorkerNode",
                columns: table => new
                {
                    WorkerID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    WorkingFolderName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TaskAssignedDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastActiveDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkerNode", x => x.WorkerID);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "WorkerNode");
        }
    }
}
