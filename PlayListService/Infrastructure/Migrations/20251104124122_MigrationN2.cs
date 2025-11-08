using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class MigrationN2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PlaylistSongs_Playlists_PlaylistId",
                table: "PlaylistSongs");

            migrationBuilder.DropForeignKey(
                name: "FK_PlaylistSongs_Songs_SongId",
                table: "PlaylistSongs");

            migrationBuilder.DropForeignKey(
                name: "FK_PlaylistSongVotes_PlaylistSongs_PlaylistId_SongId",
                table: "PlaylistSongVotes");

            migrationBuilder.DropForeignKey(
                name: "FK_SongMetadata_Songs_SongId",
                table: "SongMetadata");

            migrationBuilder.DropForeignKey(
                name: "FK_Songs_Artists_ArtistId",
                table: "Songs");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Users",
                table: "Users");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Songs",
                table: "Songs");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Playlists",
                table: "Playlists");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Artists",
                table: "Artists");

            migrationBuilder.DropPrimaryKey(
                name: "PK_SongMetadata",
                table: "SongMetadata");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PlaylistSongVotes",
                table: "PlaylistSongVotes");

            migrationBuilder.DropIndex(
                name: "IX_PlaylistSongVotes_PlaylistId_SongId",
                table: "PlaylistSongVotes");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PlaylistSongs",
                table: "PlaylistSongs");

            migrationBuilder.RenameTable(
                name: "Users",
                newName: "users");

            migrationBuilder.RenameTable(
                name: "Songs",
                newName: "songs");

            migrationBuilder.RenameTable(
                name: "Playlists",
                newName: "playlists");

            migrationBuilder.RenameTable(
                name: "Artists",
                newName: "artists");

            migrationBuilder.RenameTable(
                name: "SongMetadata",
                newName: "song_metadata");

            migrationBuilder.RenameTable(
                name: "PlaylistSongVotes",
                newName: "playlist_song_votes");

            migrationBuilder.RenameTable(
                name: "PlaylistSongs",
                newName: "playlist_songs");

            migrationBuilder.RenameIndex(
                name: "IX_Songs_ArtistId",
                table: "songs",
                newName: "IX_songs_ArtistId");

            migrationBuilder.RenameIndex(
                name: "IX_SongMetadata_SongId",
                table: "song_metadata",
                newName: "IX_song_metadata_SongId");

            migrationBuilder.RenameIndex(
                name: "IX_PlaylistSongs_SongId",
                table: "playlist_songs",
                newName: "IX_playlist_songs_SongId");

            migrationBuilder.AlterColumn<string>(
                name: "UserName",
                table: "users",
                type: "character varying(128)",
                maxLength: 128,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Role",
                table: "users",
                type: "character varying(64)",
                maxLength: 64,
                nullable: false,
                defaultValue: "User",
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "PasswordHash",
                table: "users",
                type: "character varying(512)",
                maxLength: 512,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Title",
                table: "songs",
                type: "character varying(256)",
                maxLength: 256,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Genre",
                table: "songs",
                type: "character varying(64)",
                maxLength: 64,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "playlists",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Genre",
                table: "playlists",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "playlists",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "artists",
                type: "character varying(256)",
                maxLength: 256,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Country",
                table: "artists",
                type: "character varying(128)",
                maxLength: 128,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Mood",
                table: "song_metadata",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ExternalId",
                table: "song_metadata",
                type: "character varying(128)",
                maxLength: 128,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Album",
                table: "song_metadata",
                type: "character varying(256)",
                maxLength: 256,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                table: "playlist_song_votes",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer")
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddColumn<DateTime>(
                name: "AddedAtUtc",
                table: "playlist_songs",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "NOW() AT TIME ZONE 'UTC'");

            migrationBuilder.AddColumn<int>(
                name: "AddedByUserId",
                table: "playlist_songs",
                type: "integer",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_users",
                table: "users",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_songs",
                table: "songs",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_playlists",
                table: "playlists",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_artists",
                table: "artists",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_song_metadata",
                table: "song_metadata",
                column: "SongMetadataId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_playlist_song_votes",
                table: "playlist_song_votes",
                columns: new[] { "PlaylistId", "SongId", "UserId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_playlist_songs",
                table: "playlist_songs",
                columns: new[] { "PlaylistId", "SongId" });

            migrationBuilder.UpdateData(
                table: "users",
                keyColumn: "Id",
                keyValue: 1,
                column: "UserName",
                value: "admin");

            migrationBuilder.CreateIndex(
                name: "IX_users_UserName",
                table: "users",
                column: "UserName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_playlists_Genre_Id",
                table: "playlists",
                columns: new[] { "Genre", "Id" });

            migrationBuilder.CreateIndex(
                name: "IX_artists_Name",
                table: "artists",
                column: "Name",
                unique: true);

            migrationBuilder.AddCheckConstraint(
                name: "CK_Vote_Value",
                table: "playlist_song_votes",
                sql: "\"Value\" IN (-1, 1)");

            migrationBuilder.CreateIndex(
                name: "IX_playlist_songs_PlaylistId_Order",
                table: "playlist_songs",
                columns: new[] { "PlaylistId", "Order" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_playlist_song_votes_playlist_songs_PlaylistId_SongId",
                table: "playlist_song_votes",
                columns: new[] { "PlaylistId", "SongId" },
                principalTable: "playlist_songs",
                principalColumns: new[] { "PlaylistId", "SongId" },
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_playlist_songs_playlists_PlaylistId",
                table: "playlist_songs",
                column: "PlaylistId",
                principalTable: "playlists",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_playlist_songs_songs_SongId",
                table: "playlist_songs",
                column: "SongId",
                principalTable: "songs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_song_metadata_songs_SongId",
                table: "song_metadata",
                column: "SongId",
                principalTable: "songs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_songs_artists_ArtistId",
                table: "songs",
                column: "ArtistId",
                principalTable: "artists",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_playlist_song_votes_playlist_songs_PlaylistId_SongId",
                table: "playlist_song_votes");

            migrationBuilder.DropForeignKey(
                name: "FK_playlist_songs_playlists_PlaylistId",
                table: "playlist_songs");

            migrationBuilder.DropForeignKey(
                name: "FK_playlist_songs_songs_SongId",
                table: "playlist_songs");

            migrationBuilder.DropForeignKey(
                name: "FK_song_metadata_songs_SongId",
                table: "song_metadata");

            migrationBuilder.DropForeignKey(
                name: "FK_songs_artists_ArtistId",
                table: "songs");

            migrationBuilder.DropPrimaryKey(
                name: "PK_users",
                table: "users");

            migrationBuilder.DropIndex(
                name: "IX_users_UserName",
                table: "users");

            migrationBuilder.DropPrimaryKey(
                name: "PK_songs",
                table: "songs");

            migrationBuilder.DropPrimaryKey(
                name: "PK_playlists",
                table: "playlists");

            migrationBuilder.DropIndex(
                name: "IX_playlists_Genre_Id",
                table: "playlists");

            migrationBuilder.DropPrimaryKey(
                name: "PK_artists",
                table: "artists");

            migrationBuilder.DropIndex(
                name: "IX_artists_Name",
                table: "artists");

            migrationBuilder.DropPrimaryKey(
                name: "PK_song_metadata",
                table: "song_metadata");

            migrationBuilder.DropPrimaryKey(
                name: "PK_playlist_songs",
                table: "playlist_songs");

            migrationBuilder.DropIndex(
                name: "IX_playlist_songs_PlaylistId_Order",
                table: "playlist_songs");

            migrationBuilder.DropPrimaryKey(
                name: "PK_playlist_song_votes",
                table: "playlist_song_votes");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Vote_Value",
                table: "playlist_song_votes");

            migrationBuilder.DropColumn(
                name: "AddedAtUtc",
                table: "playlist_songs");

            migrationBuilder.DropColumn(
                name: "AddedByUserId",
                table: "playlist_songs");

            migrationBuilder.RenameTable(
                name: "users",
                newName: "Users");

            migrationBuilder.RenameTable(
                name: "songs",
                newName: "Songs");

            migrationBuilder.RenameTable(
                name: "playlists",
                newName: "Playlists");

            migrationBuilder.RenameTable(
                name: "artists",
                newName: "Artists");

            migrationBuilder.RenameTable(
                name: "song_metadata",
                newName: "SongMetadata");

            migrationBuilder.RenameTable(
                name: "playlist_songs",
                newName: "PlaylistSongs");

            migrationBuilder.RenameTable(
                name: "playlist_song_votes",
                newName: "PlaylistSongVotes");

            migrationBuilder.RenameIndex(
                name: "IX_songs_ArtistId",
                table: "Songs",
                newName: "IX_Songs_ArtistId");

            migrationBuilder.RenameIndex(
                name: "IX_song_metadata_SongId",
                table: "SongMetadata",
                newName: "IX_SongMetadata_SongId");

            migrationBuilder.RenameIndex(
                name: "IX_playlist_songs_SongId",
                table: "PlaylistSongs",
                newName: "IX_PlaylistSongs_SongId");

            migrationBuilder.AlterColumn<string>(
                name: "UserName",
                table: "Users",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(128)",
                oldMaxLength: 128);

            migrationBuilder.AlterColumn<string>(
                name: "Role",
                table: "Users",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(64)",
                oldMaxLength: 64,
                oldDefaultValue: "User");

            migrationBuilder.AlterColumn<string>(
                name: "PasswordHash",
                table: "Users",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(512)",
                oldMaxLength: 512);

            migrationBuilder.AlterColumn<string>(
                name: "Title",
                table: "Songs",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(256)",
                oldMaxLength: 256);

            migrationBuilder.AlterColumn<string>(
                name: "Genre",
                table: "Songs",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(64)",
                oldMaxLength: 64);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Playlists",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(200)",
                oldMaxLength: 200);

            migrationBuilder.AlterColumn<string>(
                name: "Genre",
                table: "Playlists",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(64)",
                oldMaxLength: 64,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Playlists",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(2000)",
                oldMaxLength: 2000,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Artists",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(256)",
                oldMaxLength: 256);

            migrationBuilder.AlterColumn<string>(
                name: "Country",
                table: "Artists",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(128)",
                oldMaxLength: 128,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Mood",
                table: "SongMetadata",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(64)",
                oldMaxLength: 64,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ExternalId",
                table: "SongMetadata",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(128)",
                oldMaxLength: 128,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Album",
                table: "SongMetadata",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(256)",
                oldMaxLength: 256,
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                table: "PlaylistSongVotes",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Users",
                table: "Users",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Songs",
                table: "Songs",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Playlists",
                table: "Playlists",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Artists",
                table: "Artists",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_SongMetadata",
                table: "SongMetadata",
                column: "SongMetadataId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_PlaylistSongs",
                table: "PlaylistSongs",
                columns: new[] { "PlaylistId", "SongId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_PlaylistSongVotes",
                table: "PlaylistSongVotes",
                column: "Id");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "UserName",
                value: "demo");

            migrationBuilder.CreateIndex(
                name: "IX_PlaylistSongVotes_PlaylistId_SongId",
                table: "PlaylistSongVotes",
                columns: new[] { "PlaylistId", "SongId" });

            migrationBuilder.AddForeignKey(
                name: "FK_PlaylistSongs_Playlists_PlaylistId",
                table: "PlaylistSongs",
                column: "PlaylistId",
                principalTable: "Playlists",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PlaylistSongs_Songs_SongId",
                table: "PlaylistSongs",
                column: "SongId",
                principalTable: "Songs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PlaylistSongVotes_PlaylistSongs_PlaylistId_SongId",
                table: "PlaylistSongVotes",
                columns: new[] { "PlaylistId", "SongId" },
                principalTable: "PlaylistSongs",
                principalColumns: new[] { "PlaylistId", "SongId" },
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SongMetadata_Songs_SongId",
                table: "SongMetadata",
                column: "SongId",
                principalTable: "Songs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Songs_Artists_ArtistId",
                table: "Songs",
                column: "ArtistId",
                principalTable: "Artists",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
