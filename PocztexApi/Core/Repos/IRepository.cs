using PocztexApi.Core.Models;

namespace PocztexApi.Core.Repositories;

public interface IRepository<T> where T : IModel
{
    Task<bool> IsEmpty();

    Task<List<T>> GetAll();
    Task<T?> GetByUniqueId(UniqueId id);

    Task Create(T model);

    async Task<T> CreateAndReturn(T model)
    {
        await Create(model);
        return model;
    }

    Task DeleteByUniqueId(UniqueId id);
    Task Delete(T model) => DeleteByUniqueId(model.UniqueId);

    Task Update(T model);
}