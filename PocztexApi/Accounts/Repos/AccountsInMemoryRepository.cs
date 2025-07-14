using PocztexApi.Shared.Repositories.InMemory;

namespace PocztexApi.Accounts.Repos;

public class AccountsInMemoryRepository : CrudInMemoryRepository<Account>, IAccountsRepository
{
    public Task<List<Account>> GetByFilters(bool? locked) => Task.FromResult(Models.Where(a => locked is null || a.Locked == locked).ToList());
    public Task<Account?> GetByLogin(Login login) => Task.FromResult(Models.FirstOrDefault(m => m.Login == login));
}