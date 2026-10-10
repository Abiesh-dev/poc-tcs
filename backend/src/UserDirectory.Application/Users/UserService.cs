using Microsoft.Extensions.Logging;
using UserDirectory.Application.Abstractions;
using UserDirectory.Application.Users.Dtos;
using UserDirectory.Application.Users.Mapping;
using UserDirectory.Domain.Entities;
using UserDirectory.Domain.Exceptions;

namespace UserDirectory.Application.Users;

public sealed class UserService : IUserService
{
    private readonly IUserRepository _repository;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly ILogger<UserService> _logger;

    public UserService(
        IUserRepository repository,
        IDateTimeProvider dateTimeProvider,
        ILogger<UserService> logger)
    {
        _repository = repository;
        _dateTimeProvider = dateTimeProvider;
        _logger = logger;
    }

    public async Task<IReadOnlyList<UserResponse>> GetAllAsync(CancellationToken cancellationToken)
    {
        var users = await _repository.GetAllAsync(cancellationToken);
        return users.Select(user => user.ToResponse()).ToList();
    }

    public async Task<UserResponse> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        var user = await _repository.GetByIdAsync(id, cancellationToken)
                   ?? throw new UserNotFoundException(id);

        return user.ToResponse();
    }

    public async Task<UserResponse> CreateAsync(CreateUserRequest request, CancellationToken cancellationToken)
    {
        var name = request.Name.Trim();
        var pincode = request.Pincode.Trim();

        await EnsureNotDuplicateAsync(name, pincode, excludeId: null, cancellationToken);

        var user = new User(
            name,
            request.Age,
            request.City,
            request.State,
            pincode,
            _dateTimeProvider.UtcNow);

        await _repository.AddAsync(user, cancellationToken);

        _logger.LogInformation("Created user {UserId}", user.Id);

        return user.ToResponse();
    }

    public async Task<UserResponse> UpdateAsync(int id, UpdateUserRequest request, CancellationToken cancellationToken)
    {
        var user = await _repository.GetByIdAsync(id, cancellationToken)
                   ?? throw new UserNotFoundException(id);

        var name = request.Name.Trim();
        var pincode = request.Pincode.Trim();

        await EnsureNotDuplicateAsync(name, pincode, excludeId: id, cancellationToken);

        user.UpdateDetails(
            name,
            request.Age,
            request.City,
            request.State,
            pincode,
            _dateTimeProvider.UtcNow);

        await _repository.UpdateAsync(user, cancellationToken);

        _logger.LogInformation("Updated user {UserId}", user.Id);

        return user.ToResponse();
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken)
    {
        var user = await _repository.GetByIdAsync(id, cancellationToken)
                   ?? throw new UserNotFoundException(id);

        await _repository.DeleteAsync(user, cancellationToken);

        _logger.LogInformation("Deleted user {UserId}", id);
    }

    private async Task EnsureNotDuplicateAsync(
        string name,
        string pincode,
        int? excludeId,
        CancellationToken cancellationToken)
    {
        if (await _repository.ExistsAsync(name, pincode, excludeId, cancellationToken))
        {
            throw new DuplicateUserException(name, pincode);
        }
    }
}
