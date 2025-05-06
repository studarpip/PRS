using PRS.Model.Enums;

namespace PRS.Model.Entities
{
    public class User : UserBasic
    {
        public required string Email { get; set; }
        public required string EmailHash { get; set; }
        public Gender Gender { get; set; } = Gender.Unknown;
        public Country Country { get; set; } = Country.Unknown;
        public DateOnly DateOfBirth { get; set; }
    }
}
