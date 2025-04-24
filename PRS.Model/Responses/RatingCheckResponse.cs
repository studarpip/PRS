namespace PRS.Model.Responses
{
    public class RatingCheckResponse
    {
        public bool CanRate { get; set; }
        public int? PreviousRating { get; set; }
    }
}
