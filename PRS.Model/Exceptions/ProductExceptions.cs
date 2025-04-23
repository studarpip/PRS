namespace PRS.Model.Exceptions
{
    public class ProductNotFoundException : PrsException
    {
        public ProductNotFoundException() : base("Product not found.") { }
    }
}
