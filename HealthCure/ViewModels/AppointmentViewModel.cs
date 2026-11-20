namespace HealthCure.ViewModels
{
    public class AppointmentViewModel
    {
        public int AppointmentId { get; set; }

        public int PatientId { get; set; }

        public int DoctorId { get; set; }

        public string PatientName { get; set; } = "";

        public string DoctorName { get; set; } = "";

        public DateOnly AppointmentDate { get; set; }

        public string? Status { get; set; }

        public string? Message { get; set; }
    }
}