namespace PRS.Model.Exceptions
{
    public class ProductNotFoundException : PrsException
    {
        public ProductNotFoundException() : base("Product not found.") { }
    }

    public class PriceException : PrsException
    {
        public PriceException() : base("Price has to be higher than 0.") { }
    }
}
