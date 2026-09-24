namespace InnoClinic.AspNetCore.Authorization;

public static class ClinicPermissions
{
    public static class Appointments
    {
        public const string Read = "read:appointments";
        public const string Write = "write:appointments";
        public const string Confirm = "confirm:appointments";
    }

    public static class Results
    {
        public const string Read = "read:results";
        public const string Write = "write:results";
    }

    public static class Patients
    {
        public const string Read = "read:patients";
        public const string Write = "write:patients";
    }

    public static class Staff
    {
        public const string Read = "read:staff";
        public const string Write = "write:staff";
    }

    public static class Specializations
    {
        public const string Write = "write:specializations";
    }
}
