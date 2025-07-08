using PocztexApi.Accounts.Core.Types;
using PocztexApi.Core.Models;

namespace PocztexApi.Accounts.Core.Models;

public record Account(
    UniqueId UniqueId,
    Login Login,
    PasswordHash PasswordHash
) : IModel;
