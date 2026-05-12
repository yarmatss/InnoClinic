using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Profiles.DAL.Migrations
{
    /// <inheritdoc />
    public partial class UpdateGenderToEnum : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("ALTER TABLE \"Staff\" ALTER COLUMN \"Gender\" TYPE integer USING CASE \"Gender\" WHEN 'Male' THEN 1 WHEN 'Female' THEN 2 ELSE 1 END;");

            migrationBuilder.Sql("ALTER TABLE \"Patients\" ALTER COLUMN \"Gender\" TYPE integer USING CASE \"Gender\" WHEN 'Male' THEN 1 WHEN 'Female' THEN 2 ELSE 1 END;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("ALTER TABLE \"Staff\" ALTER COLUMN \"Gender\" TYPE text USING CASE \"Gender\" WHEN 1 THEN 'Male' WHEN 2 THEN 'Female' ELSE 'Male' END;");

            migrationBuilder.Sql("ALTER TABLE \"Patients\" ALTER COLUMN \"Gender\" TYPE text USING CASE \"Gender\" WHEN 1 THEN 'Male' WHEN 2 THEN 'Female' ELSE 'Male' END;");
        }
    }
}
