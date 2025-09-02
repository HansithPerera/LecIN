namespace LecIN_System.Models
{
    public class Admin
    {
        public int Id { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Role { get; set; } // e.g., Super Admin, Staff Admin
    }
}
