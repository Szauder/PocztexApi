using PocztexApi.Accounts.Core.Models;
using PocztexApi.Accounts.Core.Repos;
using PocztexApi.Core.Types;

public class AccountsService(IAccountsRepository repository, IPasswordHasher passwordHasher)
{
    public async Task<Account> RegisterAccount(Name name, Password password)
    {
        return await repository.CreateAndReturn(new(
            UniqueId: UniqueId.CreateNew(),
            Name: name,
            PasswordHash: passwordHasher.Hash(password)
        ));
    }
}
