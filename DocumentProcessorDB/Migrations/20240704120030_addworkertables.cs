using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DocumentProcessorDB.Migrations
{
    /// <inheritdoc />
    public partial class addworkertables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<byte>(
                name: "Status",
                table: "WorkerNode",
                type: "tinyint",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(250)",
                oldMaxLength: 250);

            migrationBuilder.CreateTable(
                name: "FolderDetails",
                columns: table => new
                {
                    FolderId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    WorkerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SourceSubFolderName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DestinationSubFolderName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MergedFileName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<byte>(type: "tinyint", nullable: false),
                    ErrorMessage = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FolderDetails", x => x.FolderId);
                    table.ForeignKey(
                        name: "FK_FolderDetails_WorkerNode_WorkerId",
                        column: x => x.WorkerId,
                        principalTable: "WorkerNode",
                        principalColumn: "WorkerID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FileDetails",
                columns: table => new
                {
                    FileId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FolderDetailsId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SourceFileName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<byte>(type: "tinyint", nullable: false),
                    ErrorMessage = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FileDetails", x => x.FileId);
                    table.ForeignKey(
                        name: "FK_FileDetails_FolderDetails_FolderDetailsId",
                        column: x => x.FolderDetailsId,
                        principalTable: "FolderDetails",
                        principalColumn: "FolderId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FileDetails_FolderDetailsId",
                table: "FileDetails",
                column: "FolderDetailsId");

            migrationBuilder.CreateIndex(
                name: "IX_FolderDetails_WorkerId",
                table: "FolderDetails",
                column: "WorkerId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FileDetails");

            migrationBuilder.DropTable(
                name: "FolderDetails");

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "WorkerNode",
                type: "nvarchar(250)",
                maxLength: 250,
                nullable: false,
                oldClrType: typeof(byte),
                oldType: "tinyint");
        }
    }
}
