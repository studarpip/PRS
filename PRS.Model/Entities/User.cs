using PRS.Model.Enums;

namespace PRS.Model.Entities
{
    public class User
    {
        public Guid Id { get; set; }
        public required string Username { get; set; }
        public required string Email { get; set; }
        public required string Password { get; set; }
        public Gender Gender { get; set; }
        public Country Country { get; set; }
        public Role Role { get; set; }
    }
}
