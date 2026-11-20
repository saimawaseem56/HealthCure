using System;
using System.Collections.Generic;

namespace HealthCure.Models;

public partial class ContactMessage
{
    public int ContactId { get; set; }

    public string Name { get; set; } = null!;

    public string? Email { get; set; }

    public string? Subject { get; set; }

    public string? Message { get; set; }

    public DateTime? CreatedDate { get; set; }

    public bool? IsRead { get; set; }
}
