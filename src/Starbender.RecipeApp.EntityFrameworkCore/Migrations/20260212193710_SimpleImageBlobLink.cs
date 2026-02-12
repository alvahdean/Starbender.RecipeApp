using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Starbender.RecipeApp.EntityFrameworkCore.Migrations
{
    /// <inheritdoc />
    public partial class SimpleImageBlobLink : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Recipes_BlobMetadata_ImageMetadataId",
                table: "Recipes");

            migrationBuilder.DropIndex(
                name: "IX_Recipes_ImageMetadataId",
                table: "Recipes");

            migrationBuilder.RenameColumn(
                name: "ImageMetadataId",
                table: "Recipes",
                newName: "ImageBlobId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ImageBlobId",
                table: "Recipes",
                newName: "ImageMetadataId");

            migrationBuilder.CreateIndex(
                name: "IX_Recipes_ImageMetadataId",
                table: "Recipes",
                column: "ImageMetadataId");

            migrationBuilder.AddForeignKey(
                name: "FK_Recipes_BlobMetadata_ImageMetadataId",
                table: "Recipes",
                column: "ImageMetadataId",
                principalTable: "BlobMetadata",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
