namespace PRS.Model.Entities
{
    public class RecommendationSettings
    {
        public bool UseContent { get; set; }
        public bool UseCollaborative { get; set; }
        public double CategoryWeight { get; set; }
        public double PriceWeight { get; set; }
        public double BrowseWeight { get; set; }
        public double ViewWeight { get; set; }
        public double CartWeight { get; set; }
        public double PurchaseWeight { get; set; }
        public double RatingWeight { get; set; }
    }
}
