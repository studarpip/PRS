using PRS.Model.Attributes;

namespace PRS.Model.Enums
{
    public enum Gender
    {
        [SkipRelationship]
        Unknown = 0,
        Male = 1,
        Female = 2,
        Other = 3,
    }
}
