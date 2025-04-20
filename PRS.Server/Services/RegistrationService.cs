using PRS.Model.Entities;
using PRS.Model.Enums;
using PRS.Model.Exceptions;
using PRS.Model.Requests;
using PRS.Model.Responses;
using PRS.Server.Helpers;
using PRS.Server.Helpers.Interfaces;
using PRS.Server.Repositories.Interfaces;
using PRS.Server.Services.Interfaces;

namespace PRS.Server.Services
{
    public class RegistrationService : IRegistrationService
    {
        private readonly IRegistrationRepository _repository;
        private readonly IEncryptionHelper _encryption;

        public RegistrationService(IRegistrationRepository repository, IEncryptionHelper encryption)
        {
            _repository = repository;
            _encryption = encryption;
        }

        public async Task<ServerResponse> RegisterAsync(RegistrationRequest request)
        {
            if (!request.Password.IsStrongPassword())
                throw new WeakPasswordException();

            if (!request.Email.IsValidEmail())
                throw new InvalidEmailFormatException();

            request.Email.ToLower();

            var emailHash = request.Email.HashString();

            if (await _repository.UsernameExistsAsync(request.Username))
                throw new UsernameAlreadyExistsException();

            if (await _repository.EmailExistsAsync(emailHash))
                throw new EmailAlreadyExistsException();

            var user = new User
            {
                Id = Guid.NewGuid(),
                Username = request.Username,
                Email = _encryption.Encrypt(request.Email),
                EmailHash = emailHash,
                Password = request.Password.HashString(),
                Country = request.Country,
                Gender = request.Gender,
                Role = Role.User,
                DateOfBirth = DateOnly.FromDateTime(request.DateOfBirth)
            };

            await _repository.CreateAsync(user);

            return ServerResponse.Ok();
        }
    }
}
