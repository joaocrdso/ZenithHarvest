namespace ZenithHarvest.Domain.Interfaces;

using Entities;

public interface IInsurerRepository
{
    Task<Insurer?> GetByIdAsync(int id);
    Task<IEnumerable<Insurer>> GetAllAsync();
    Task AddAsync(Insurer insurer);
}

public interface IPolicyRepository
{
    Task<Policy?> GetByIdAsync(int id);
    Task<IEnumerable<Policy>> GetByInsurerIdAsync(int insurerId);
    Task<IEnumerable<Policy>> GetAllAsync();
    Task AddAsync(Policy policy);
}

public interface IClaimRepository
{
    Task<Claim?> GetByIdAsync(int id);
    Task<IEnumerable<Claim>> GetByPolicyIdAsync(int policyId);
    Task<IEnumerable<Claim>> GetAllAsync();
    Task AddAsync(Claim claim);
}

public interface IUserRepository
{
    Task<User?> GetByEmailAsync(string email);
    Task<User?> GetByIdAsync(int id);
    Task AddAsync(User user);
}
