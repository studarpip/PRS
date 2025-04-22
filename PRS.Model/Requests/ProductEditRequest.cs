using PRS.Model.Enums;

namespace PRS.Model.Requests
{
    public class ProductEditRequest
    {
        public required string Name { get; set; }
        public string? Description { get; set; }
        public List<Category> Categories { get; set; } = new();
        public byte[]? Image { get; set; }
    }
}
