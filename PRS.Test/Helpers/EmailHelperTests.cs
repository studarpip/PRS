using FluentAssertions;
using PRS.Server.Helpers;

public class EmailHelperTests
{
    [Theory]
    [InlineData("test@example.com", true)]
    [InlineData("user.name@domain.co.uk", true)]
    [InlineData("user+name@domain.io", true)]
    [InlineData("invalid-email", false)]
    [InlineData("user@.com", false)]
    [InlineData("user@domain", false)]
    [InlineData("@nodomain.com", false)]
    [InlineData(" ", false)]
    [InlineData("", false)]
    [InlineData(null, false)]
    public void IsValidEmail_ShouldReturnExpectedResult(string email, bool expected)
    {
        var result = email.IsValidEmail();
        result.Should().Be(expected);
    }
}
