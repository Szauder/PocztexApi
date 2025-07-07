using PocztexApi.Accounts.Core.Models;
using PocztexApi.Accounts.Core.Repos;
using PocztexApi.Core.Types;
using PocztexApi.Shared.Repositories.InMemory;

namespace PocztexApi.Accounts.Repos;

public class AccountsInMemoryRepository : CrudInMemoryRepository<Account>, IAccountsRepository
{
    public Task<Account?> GetByName(Name name) => Task.FromResult(Models.FirstOrDefault(m => m.Name == name));
}