namespace ZenithHarvest.Infrastructure.Repositories;

using ZenithHarvest.Domain.Entities;
using ZenithHarvest.Domain.Interfaces;
using Persistence;

// SRP: cada repositório responsável só por uma entidade
public class InsurerRepository : IInsurerRepository
{
    private readonly ZenithContext _context;

    public InsurerRepository(ZenithContext context)
    {
        _context = context;
    }

    public async Task<Insurer?> GetByIdAsync(int id) =>
        await _context.Insurers.FindAsync(id);

    public async Task<IEnumerable<Insurer>> GetAllAsync() =>
        _context.Insurers.ToList();

    public async Task AddAsync(Insurer insurer)
    {
        await _context.Insurers.AddAsync(insurer);
        await _context.SaveChangesAsync();
    }
}

public class PolicyRepository : IPolicyRepository
{
    private readonly ZenithContext _context;

    public PolicyRepository(ZenithContext context)
    {
        _context = context;
    }

    public async Task<Policy?> GetByIdAsync(int id) =>
        await _context.Policies.FindAsync(id);

    public async Task<IEnumerable<Policy>> GetByInsurerIdAsync(int insurerId) =>
        _context.Policies.Where(p => p.InsurerId == insurerId).ToList();

    public async Task<IEnumerable<Policy>> GetAllAsync() =>
        _context.Policies.ToList();

    public async Task AddAsync(Policy policy)
    {
        await _context.Policies.AddAsync(policy);
        await _context.SaveChangesAsync();
    }
}

public class ClaimRepository : IClaimRepository
{
    private readonly ZenithContext _context;

    public ClaimRepository(ZenithContext context)
    {
        _context = context;
    }

    public async Task<Claim?> GetByIdAsync(int id) =>
        await _context.Claims.FindAsync(id);

    public async Task<IEnumerable<Claim>> GetByPolicyIdAsync(int policyId) =>
        _context.Claims.Where(c => c.PolicyId == policyId).ToList();

    public async Task<IEnumerable<Claim>> GetAllAsync() =>
        _context.Claims.ToList();

    public async Task AddAsync(Claim claim)
    {
        await _context.Claims.AddAsync(claim);
        await _context.SaveChangesAsync();
    }
}

public class UserRepository : IUserRepository
{
    private readonly ZenithContext _context;

    public UserRepository(ZenithContext context)
    {
        _context = context;
    }

    public async Task<User?> GetByEmailAsync(string email) =>
        _context.Users.FirstOrDefault(u => u.Email == email);

    public async Task<User?> GetByIdAsync(int id) =>
        await _context.Users.FindAsync(id);

    public async Task AddAsync(User user)
    {
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();
    }
}
