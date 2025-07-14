namespace PocztexApi.Accounts.Core.Repos;

public interface IAccountsRepository : IRepository<Account>
{
    Task<Account?> GetByLogin(Login login);
    Task<List<Account>> GetByFilters(bool? locked);
}
