using PRS.Model.Entities;

namespace PRS.Server.Repositories.Interfaces
{
    public interface IAuthRepository
    {
        Task<UserBasic?> GetByUsernameAsync(string username);
    }
}
