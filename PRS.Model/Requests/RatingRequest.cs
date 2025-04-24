namespace PRS.Model.Requests
{
    public class RatingRequest
    {
        public Guid ProductId { get; set; }
        public int Rating { get; set; }
    }
}
