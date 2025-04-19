namespace PRS.Model.Exceptions
{
    public class EmailAlreadyExistsException : PrsException
    {
        public EmailAlreadyExistsException() : base("Email already in use") { }
    }

    public class UsernameAlreadyExistsException : PrsException
    {
        public UsernameAlreadyExistsException() : base("Username already exists") { }
    }

    public class InvalidEmailFormatException : PrsException
    {
        public InvalidEmailFormatException() : base("Email is not valid") { }
    }

    public class WeakPasswordException : PrsException
    {
        public WeakPasswordException() : base("Password is too weak") { }
    }
}
