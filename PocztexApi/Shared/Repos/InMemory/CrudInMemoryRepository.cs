using PocztexApi.Core.Models;

namespace PocztexApi.Shared.Repositories.InMemory;

public class CrudInMemoryRepository<TModel> : IRepository<TModel> where TModel : IModel
{
    protected List<TModel> Models { get; init; } = [];

    public Task<bool> IsEmpty() => Task.FromResult(Models.Count == 0);

    public Task<TModel?> GetByUniqueId(UniqueId id) => Task.FromResult(Models.FirstOrDefault(m => m.UniqueId == id));

    public async Task Create(TModel entity)
    {
        if (await GetByUniqueId(entity.UniqueId) is not null)
            throw new AppException();

        Models.Add(entity);
    }

    public Task DeleteByUniqueId(UniqueId id)
    {
        Models.RemoveAll(e => e.UniqueId == id);
        return Task.CompletedTask;
    }

    public Task Update(TModel entity)
    {
        Models.RemoveAll(e => e.UniqueId == entity.UniqueId);
        Models.Add(entity);

        return Task.CompletedTask;
    }

    public Task<List<TModel>> GetAll() => Task.FromResult(new List<TModel>(Models));
}