using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CMGWpf.Migrations
{
    /// <inheritdoc />
    public partial class AddChordSequenceTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "chordsequence",
                columns: table => new
                {
                    Name = table.Column<string>(type: "TEXT", maxLength: 45, nullable: false),
                    SoundFontFileName = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    PresetName = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                    BPM = table.Column<double>(type: "REAL", nullable: false),
                    RootNote = table.Column<string>(type: "TEXT", maxLength: 1, nullable: false),
                    RootOctave = table.Column<int>(type: "INTEGER", nullable: false),
                    IsMajor = table.Column<bool>(type: "INTEGER", nullable: false),
                    Effort = table.Column<string>(type: "TEXT", nullable: false),
                    Items = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_chordsequence", x => x.Name);
                });

        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "chordsequence");

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
