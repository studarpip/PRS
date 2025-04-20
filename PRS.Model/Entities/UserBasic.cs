using PRS.Model.Enums;

namespace PRS.Model.Entities
{
    public class UserBasic
    {
        public Guid Id { get; set; }
        public required string Username { get; set; }
        public required string Password { get; set; }
        public Role Role { get; set; } = Role.User;
    }
}
