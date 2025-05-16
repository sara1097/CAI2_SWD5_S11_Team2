using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class seedProduct : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Products",
                columns: new[] { "Id", "CategoryId", "Description", "Discount", "ImageUrl", "IsFeatured", "Name", "Price", "StockQuantity" },
                values: new object[,]
                {
                    { 1, 1, "High-performance laptop", 0.10m, "~/assets/img/products/laptop.jpg", true, "Laptop", 999.99m, 10 },
                    { 2, 1, "Latest smartphone model", 0.05m, "~/assets/img/products/smartphone.jpg", true, "Smartphone", 699.99m, 15 },
                    { 3, 2, "Bestselling novel", 0.00m, "~/assets/img/products/novel.jpg", false, "Novel", 19.99m, 20 },
                    { 4, 3, "Comfortable cotton t-shirt", 0.00m, "~/assets/img/products/tshirt.jpg", false, "T-Shirt", 29.99m, 25 }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 4);
        }
    }
}
