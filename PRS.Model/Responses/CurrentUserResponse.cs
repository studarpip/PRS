using PRS.Model.Enums;

namespace PRS.Model.Responses
{
    public class CurrentUserResponse
    {
        public required string Username { get; set; }
        public Guid UserId { get; set; }
        public Role Role { get; set; }
    }
}
