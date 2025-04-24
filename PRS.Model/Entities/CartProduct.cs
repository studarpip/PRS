namespace PRS.Model.Entities
{
    public class CartProduct
    {
        public Guid ProductId { get; set; }
        public required string Name { get; set; }
        public byte[]? Image { get; set; }
        public decimal Price { get; set; }
        public int Count { get; set; }
    }
}
