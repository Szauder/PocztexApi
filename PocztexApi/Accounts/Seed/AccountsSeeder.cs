namespace PocztexApi.Accounts.Seed;

public class AccountsSeeder(SeedReader reader, IAccountsRepository accountsRepository, IPasswordHasher passwordHasher) : ISeeder
{
    public Task<bool> ShouldSeed() => accountsRepository.IsEmpty();

    public async Task Seed()
    {
        foreach (var dto in await reader.ReadSeedFromArray<AccountSeedDto>("account"))
            await accountsRepository.Create(dto.ToAccount(passwordHasher));
    }

    record AccountSeedDto(string UniqueId, string Login, string Password)
    {
        public Account ToAccount(IPasswordHasher passwordHasher) => new(
            UniqueId: SeederHelper.GetUniqueId(UniqueId),
            Login: Login,
            PasswordHash: passwordHasher.Hash(Password)
        );
    }
}