using PRS.Model.Attributes;

namespace PRS.Model.Enums
{
    public enum Country
    {
        [SkipRelationship]
        Unknown = 0,
        Lithuania = 1,
        Latvia = 2,
        Estonia = 3,
    }
}
