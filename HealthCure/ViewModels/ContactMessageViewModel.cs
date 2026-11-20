using System.ComponentModel.DataAnnotations;

namespace HealthCure.ViewModels
{
    public class ContactMessageViewModel
    {
        [Required]
        public string Name { get; set; } = "";

        [EmailAddress]
        public string? Email { get; set; }

        public string? Subject { get; set; }

        [Required]
        public string Message { get; set; } = "";
    }
}