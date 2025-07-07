using PocztexApi.Core.Models;
using PocztexApi.Core.Types;

namespace PocztexApi.Accounts.Core.Models;

public record Account(
    UniqueId UniqueId,
    Name Name,
    PasswordHash PasswordHash
) : IModel;