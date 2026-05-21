using Appointments.Domain.Enums;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Appointments.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class FinalizeAppointmentsSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "DoctorId",
                table: "TimeSlots",
                newName: "MedicalStaffId");

            migrationBuilder.RenameColumn(
                name: "DoctorId",
                table: "Appointments",
                newName: "MedicalStaffId");

            migrationBuilder.Sql("CREATE EXTENSION IF NOT EXISTS btree_gist;");

            var cancelledStatus = (int)AppointmentStatus.Cancelled;
            migrationBuilder.Sql($@"
                ALTER TABLE ""Appointments"" ADD CONSTRAINT ""EXCLUDE_OverlappingAppointments""
                EXCLUDE USING gist (
                    ""MedicalStaffId"" WITH =,
                    tstzrange(""StartTime"", ""EndTime"") WITH &&
                ) WHERE (""Status"" != {cancelledStatus});
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("ALTER TABLE \"Appointments\" DROP CONSTRAINT \"EXCLUDE_OverlappingAppointments\";");
            migrationBuilder.Sql("DROP EXTENSION IF EXISTS btree_gist;");

            migrationBuilder.RenameColumn(
                name: "MedicalStaffId",
                table: "TimeSlots",
                newName: "DoctorId");

            migrationBuilder.RenameColumn(
                name: "MedicalStaffId",
                table: "Appointments",
                newName: "DoctorId");
        }
    }
}
