using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ContentAggregator.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Feature",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    FirstNameEng = table.Column<string>(type: "text", nullable: false),
                    LastNameEng = table.Column<string>(type: "text", nullable: false),
                    FirstNameGeo = table.Column<string>(type: "text", nullable: false),
                    LastNameGeo = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Feature", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "YTChannel",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    Url = table.Column<string>(type: "text", nullable: false),
                    ActivityLevel = table.Column<byte>(type: "smallint", nullable: false),
                    LastPublishedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    TitleKeywords = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_YTChannel", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "YoutubeContent",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    VideoId = table.Column<string>(type: "text", nullable: false),
                    VideoTitle = table.Column<string>(type: "text", nullable: false),
                    ChannelId = table.Column<string>(type: "character varying(100)", nullable: false),
                    VideoLength = table.Column<TimeSpan>(type: "interval", nullable: false),
                    VideoPublishedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    NotRelevant = table.Column<bool>(type: "boolean", nullable: false),
                    FbPostedStatus = table.Column<bool>(type: "boolean", nullable: false),
                    SubtitlesEngSRT = table.Column<string>(type: "text", nullable: true),
                    SubtitlesFiltered = table.Column<string>(type: "text", nullable: true),
                    VideoSummaryEng = table.Column<string>(type: "text", nullable: true),
                    VideoSummaryGeo = table.Column<string>(type: "text", nullable: true),
                    AdditionalComments = table.Column<string>(type: "text", nullable: true),
                    FbPosted = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_YoutubeContent", x => x.Id);
                    table.ForeignKey(
                        name: "FK_YoutubeContent_YTChannel_ChannelId",
                        column: x => x.ChannelId,
                        principalTable: "YTChannel",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "YoutubeContentFeature",
                columns: table => new
                {
                    YoutubeContentId = table.Column<int>(type: "integer", nullable: false),
                    FeatureId = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_YoutubeContentFeature", x => new { x.FeatureId, x.YoutubeContentId });
                    table.ForeignKey(
                        name: "FK_YoutubeContentFeature_Feature_FeatureId",
                        column: x => x.FeatureId,
                        principalTable: "Feature",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_YoutubeContentFeature_YoutubeContent_YoutubeContentId",
                        column: x => x.YoutubeContentId,
                        principalTable: "YoutubeContent",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_YoutubeContent_ChannelId",
                table: "YoutubeContent",
                column: "ChannelId");

            migrationBuilder.CreateIndex(
                name: "IX_YoutubeContentFeature_YoutubeContentId",
                table: "YoutubeContentFeature",
                column: "YoutubeContentId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "YoutubeContentFeature");

            migrationBuilder.DropTable(
                name: "Feature");

            migrationBuilder.DropTable(
                name: "YoutubeContent");

            migrationBuilder.DropTable(
                name: "YTChannel");
        }
    }
}
