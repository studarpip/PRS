using PRS.Model.Entities;

namespace PRS.Server.Repositories.Interfaces
{
    public interface IRegistrationRepository
    {
        Task<bool> UsernameExistsAsync(string username);
        Task<bool> EmailExistsAsync(string encryptedEmail);
        Task CreateAsync(User user);
    }
}
