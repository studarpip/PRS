using PRS.Model.Enums;

namespace PRS.Model.Requests
{
    public class ProductSearchRequest
    {
        public string? Input { get; set; }
        public List<Category>? Categories { get; set; }
        public decimal? PriceFrom { get; set; }
        public decimal? PriceTo { get; set; }
        public double? RatingFrom { get; set; }
        public double? RatingTo { get; set; }
        public ProductOrderBy? OrderBy { get; set; }
        public int? Page {  get; set; }
        public int? PageSize { get; set; }
    }
}
