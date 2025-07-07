using PocztexApi.Core.Types;

namespace PocztexApi.Auth.Authenticate;

public record AuthTokenConfig(
    Secret Key
);
