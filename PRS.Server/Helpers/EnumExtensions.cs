using PRS.Model.Attributes;
using System.Reflection;

namespace PRS.Server.Helpers
{
    public static class EnumExtensions
    {
        public static bool ShouldSkipRelationship(this Enum value)
        {
            var member = value.GetType().GetMember(value.ToString()).FirstOrDefault();
            return member?.GetCustomAttribute<SkipRelationshipAttribute>() != null;
        }
    }
}
