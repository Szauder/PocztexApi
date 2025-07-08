using PocztexApi.Core.Seeding;

public class SeederBackgroundProces(IEnumerable<ISeeder> seeders) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var shouldSeed = true;

        foreach (var seeder in seeders)
        {
            if (await seeder.ShouldSeed() == false)
            {
                shouldSeed = false;
                break;
            }
        }

        if (shouldSeed)
        {
            foreach (var seeder in seeders)
                await seeder.Seed();
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
