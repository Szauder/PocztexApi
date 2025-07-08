using PocztexApi.Shared.Repositories.InMemory;

namespace PocztexApi.Accounts.Repos;

public class AccountsInMemoryRepository : CrudInMemoryRepository<Account>, IAccountsRepository
{
    public Task<Account?> GetByLogin(Login login) => Task.FromResult(Models.FirstOrDefault(m => m.Login == login));
}