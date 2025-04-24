namespace PRS.Model.Exceptions
{
    public class CannotRateProductException : PrsException
    {
        public CannotRateProductException() : base("Cannot rate product.") { }
    }
}
