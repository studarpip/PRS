using PRS.Model.Enums;

namespace PRS.Model.Entities
{
    public class Product
    {
        public Guid Id { get; set; }
        public required string Name { get; set; }
        public string? Description { get; set; }
        public List<Category> Categories { get; set; } = new();
        public byte[]? Image { get; set; }
        public decimal Price { get; set; }
        public decimal? Rating { get; set; }
        public int? RatingCount { get; set; }
    }
}
