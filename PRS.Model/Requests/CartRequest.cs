namespace PRS.Model.Requests
{
    public class CartRequest
    {
        public Guid ProductId { get; set; }
        public int Count { get; set; }
    }
}
