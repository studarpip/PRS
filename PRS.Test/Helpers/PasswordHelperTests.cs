using FluentAssertions;
using PRS.Server.Helpers;

public class PasswordHelperTests
{
    [Theory]
    [InlineData("Password1!", true)]
    [InlineData("Abcdef1@", true)]
    [InlineData("abcDEF123!", true)]
    [InlineData("short1!", false)]
    [InlineData("nouppercase1!", false)]
    [InlineData("NOLOWERCASE1!", false)]
    [InlineData("NoNumber!", false)]
    [InlineData("NoSpecial1", false)]
    [InlineData("", false)]
    [InlineData("       ", false)]
    [InlineData(null, false)]
    public void IsStrongPassword_ShouldReturnExpectedResult(string password, bool expected)
    {
        var result = password.IsStrongPassword();
        result.Should().Be(expected);
    }
}
