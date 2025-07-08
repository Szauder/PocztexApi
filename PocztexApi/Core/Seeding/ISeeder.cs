namespace PocztexApi.Core.Seeding;

public interface ISeeder
{
    Task<bool> ShouldSeed();
    Task Seed();
}