using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ContentAggregator.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddNeedsRefetchToYoutubeContent : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "FbPostedStatus",
                table: "YoutubeContent",
                newName: "NeedsRefetch");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "NeedsRefetch",
                table: "YoutubeContent",
                newName: "FbPostedStatus");
        }
    }
}
