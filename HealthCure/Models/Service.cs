using System;
using System.Collections.Generic;

namespace HealthCure.Models;

public partial class Service
{
    public int ServiceId { get; set; }

    public string Title { get; set; } = null!;

    public string? Description { get; set; }

    public string? Icon { get; set; }

    public bool? IsActive { get; set; }
}
