namespace PRS.Model.Exceptions
{
    public class CartIsEmptyException : PrsException
    {
        public CartIsEmptyException() : base("Cart is empty.") { }
    }
}
