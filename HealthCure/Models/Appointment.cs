using System;
using System.Collections.Generic;

namespace HealthCure.Models;

public partial class Appointment
{
    public int AppointmentId { get; set; }

    public int PatientId { get; set; }

    public int DoctorId { get; set; }

    public DateOnly AppointmentDate { get; set; }

    public string? Message { get; set; }

    public string? Status { get; set; }

    public DateTime? CreatedDate { get; set; }
}
