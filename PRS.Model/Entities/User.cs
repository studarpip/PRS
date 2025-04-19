using PRS.Model.Enums;

namespace PRS.Model.Entities
{
    public class User
    {
        public Guid Id { get; set; }
        public required string Username { get; set; }
        public required string Email { get; set; }
        public required string EmailHash { get; set; }
        public required string Password { get; set; }
        public Gender Gender { get; set; } = Gender.Unknown;
        public Country Country { get; set; } = Country.Unknown;
        public Role Role { get; set; } = Role.User;
        public DateOnly? DateOfBirth { get; set; }
    }
}
