using HealthCure.Models;

namespace HealthCure.ViewModels
{
    public class HomePageViewModel
    {
        public List<Doctor> Doctors { get; set; } = new();

        public List<Service> Services { get; set; } = new();

        public List<Department> Departments { get; set; } = new();
        public ContactMessageViewModel Contact { get; set; } = new();
    }
}