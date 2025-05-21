using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Moq;
using PRS.Server.Helpers;
using PRS.Server.Helpers.Interfaces;

public class EncryptionHelperTests
{
    private readonly IEncryptionHelper _encryptionHelper;

    public EncryptionHelperTests()
    {
        var mockConfig = new Mock<IConfiguration>();
        mockConfig.Setup(c => c["Encryption:Key"]).Returns("my-super-secret-key");

        _encryptionHelper = new EncryptionHelper(mockConfig.Object);
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenKeyIsMissing()
    {
        var mockConfig = new Mock<IConfiguration>();
        mockConfig.Setup(c => c["Encryption:Key"]).Returns<string>(null);

        Action act = () => new EncryptionHelper(mockConfig.Object);

        act.Should().Throw<Exception>()
           .WithMessage("Encryption key is missing from configuration.");
    }

    [Theory]
    [InlineData("test")]
    [InlineData("another test value")]
    [InlineData("1234567890")]
    public void EncryptDecrypt_ShouldReturnOriginalPlainText(string input)
    {
        var encrypted = _encryptionHelper.Encrypt(input);
        encrypted.Should().NotBeNullOrWhiteSpace();

        var decrypted = _encryptionHelper.Decrypt(encrypted);
        decrypted.Should().Be(input);
    }

    [Fact]
    public void Decrypt_ShouldThrow_WhenInputIsInvalid()
    {
        var invalidInput = "invalid-base64";

        Action act = () => _encryptionHelper.Decrypt(invalidInput);

        act.Should().Throw<FormatException>();
    }
}
