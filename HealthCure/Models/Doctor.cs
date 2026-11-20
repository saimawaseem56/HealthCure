using System;
using System.Collections.Generic;

namespace HealthCure.Models;

public partial class Doctor
{
    public int DoctorId { get; set; }

    public string FullName { get; set; } = null!;

    public string? Specialization { get; set; }

    public string? Qualification { get; set; }

    public int? Experience { get; set; }

    public string? Phone { get; set; }

    public string? Email { get; set; }

    public string? ImageUrl { get; set; }

    public string? Description { get; set; }

    public int? DepartmentId { get; set; }

    public bool? IsActive { get; set; }

    public virtual Department? Department { get; set; }
}
