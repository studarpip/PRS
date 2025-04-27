using PRS.Model.Enums;

namespace PRS.Model.Requests
{
    public class RegistrationRequest
    {
        public required string Username { get; set; }
        public required string Email { get; set; }
        public required string Password { get; set; }
        public Gender? Gender { get; set; }
        public Country? Country { get; set; }
        public DateTime DateOfBirth { get; set; }
    }
}
