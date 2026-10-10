using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccessLayer.Migrations
{
    public partial class UseDecimalProductPrices : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Check existing data before changing the column. SQL Server rounds
            // legacy real values to two decimal places during the conversion.
            migrationBuilder.Sql(@"
IF EXISTS (SELECT 1 FROM [Products]
           WHERE [Price] < 0 OR TRY_CONVERT(decimal(18,2), [Price]) IS NULL)
    THROW 50001, 'Product prices must fit the nonnegative decimal(18,2) range before migration.', 1;");

            migrationBuilder.AlterColumn<decimal>(
                name: "Price",
                table: "Products",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                oldClrType: typeof(float),
                oldType: "real");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Returning to real loses decimal precision; restore a backup if
            // the exact original values are required.
            migrationBuilder.AlterColumn<float>(
                name: "Price",
                table: "Products",
                type: "real",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldPrecision: 18,
                oldScale: 2);
        }
    }
}
