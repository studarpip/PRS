namespace PRS.Model.Exceptions
{
    public class UserNotFoundException : PrsException
    {
        public UserNotFoundException() : base("User not found.") { }
    }

    public class IncorrectPasswordException : PrsException
    {
        public IncorrectPasswordException() : base("Incorrect password.") { }
    }
}
