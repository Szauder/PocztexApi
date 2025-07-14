using PocztexApi.Core.Models;

namespace PocztexApi.Accounts.Core.Models;

public record Account(
    UniqueId UniqueId,
    bool Locked,
    Login Login,
    PasswordHash PasswordHash,
    RolesSet Roles
) : IModel;

public record RolesSet(
    bool IsAdmin,
    bool IsCustomerService,
    bool IsDelivery
);
