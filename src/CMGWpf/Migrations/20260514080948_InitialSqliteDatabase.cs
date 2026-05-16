using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CMGWpf.Migrations
{
    /// <inheritdoc />
    public partial class InitialSqliteDatabase : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ensemble",
                columns: table => new
                {
                    Name = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ensemble", x => x.Name);
                });

            migrationBuilder.CreateTable(
                name: "notesequence",
                columns: table => new
                {
                    Name = table.Column<string>(type: "TEXT", maxLength: 45, nullable: false),
                    Items = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_notesequence", x => x.Name);
                });

            migrationBuilder.CreateTable(
                name: "tag",
                columns: table => new
                {
                    Name = table.Column<string>(type: "TEXT", maxLength: 45, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tag", x => x.Name);
                });

            migrationBuilder.CreateTable(
                name: "voice",
                columns: table => new
                {
                    Name = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: false),
                    Timbre = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    RegisterLo = table.Column<float>(type: "REAL", nullable: false),
                    RegisterHi = table.Column<float>(type: "REAL", nullable: false),
                    Duration = table.Column<float>(type: "REAL", nullable: false),
                    SoundFontFile = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    PresetName = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_voice", x => x.Name);
                });

            migrationBuilder.CreateTable(
                name: "notesequence_tag",
                columns: table => new
                {
                    notesequence_name = table.Column<string>(type: "TEXT", nullable: false),
                    tag_name = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_notesequence_tag", x => new { x.notesequence_name, x.tag_name });
                    table.ForeignKey(
                        name: "FK_notesequence_tag_notesequence_notesequence_name",
                        column: x => x.notesequence_name,
                        principalTable: "notesequence",
                        principalColumn: "Name",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_notesequence_tag_tag_tag_name",
                        column: x => x.tag_name,
                        principalTable: "tag",
                        principalColumn: "Name",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ensemble_voice",
                columns: table => new
                {
                    ensemble_name = table.Column<string>(type: "TEXT", nullable: false),
                    voice_name = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ensemble_voice", x => new { x.ensemble_name, x.voice_name });
                    table.ForeignKey(
                        name: "FK_ensemble_voice_ensemble_ensemble_name",
                        column: x => x.ensemble_name,
                        principalTable: "ensemble",
                        principalColumn: "Name",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ensemble_voice_voice_voice_name",
                        column: x => x.voice_name,
                        principalTable: "voice",
                        principalColumn: "Name",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ensemble_voice_voice_name",
                table: "ensemble_voice",
                column: "voice_name");

            migrationBuilder.CreateIndex(
                name: "IX_notesequence_tag_tag_name",
                table: "notesequence_tag",
                column: "tag_name");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ensemble_voice");

            migrationBuilder.DropTable(
                name: "notesequence_tag");

            migrationBuilder.DropTable(
                name: "ensemble");

            migrationBuilder.DropTable(
                name: "voice");

            migrationBuilder.DropTable(
                name: "notesequence");

            migrationBuilder.DropTable(
                name: "tag");
        }
    }
}
