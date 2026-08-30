using SMC.Domain.Entities;

namespace SMC.Application.Interfaces;

public interface ITokenService
{
    string GenerateToken(User user);
}
