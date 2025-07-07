using PocztexApi.Accounts.Core.Models;
using PocztexApi.Core.Repositories;
using PocztexApi.Core.Types;

namespace PocztexApi.Accounts.Core.Repos;

public interface IAccountsRepository : IRepository<Account>
{
    Task<Account?> GetByName(Name name);
}