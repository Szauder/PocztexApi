public class AccountsService(IAccountsRepository repository, IPasswordHasher passwordHasher)
{
    public async Task<Account> RegisterAccount(Login login, Password password)
    {
        if (await repository.GetByLogin(login) is not null)
            throw new AppException("Account with this same login already exist");

        return await repository.CreateAndReturn(new(
            UniqueId: UniqueId.CreateNew(),
            Login: login,
            PasswordHash: passwordHasher.Hash(password)
        ));
    }
}
