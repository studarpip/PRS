namespace PRS.Server.Helpers.Interfaces
{
    public interface IEncryptionHelper
    {
        string Encrypt(string plainText);
        string Decrypt(string cipherText);
    }
}
