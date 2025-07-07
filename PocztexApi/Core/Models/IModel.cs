using PocztexApi.Core.Types;

namespace PocztexApi.Core.Models;

public interface IModel
{
    public UniqueId UniqueId { get; }
}