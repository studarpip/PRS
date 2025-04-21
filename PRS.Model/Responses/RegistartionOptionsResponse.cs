using PRS.Model.Entities;

namespace PRS.Model.Responses
{
    public class RegistrationOptionsResponse
    {
        public List<EnumOption> Genders { get; set; } = new();
        public List<EnumOption> Countries { get; set; } = new();
    }
}
