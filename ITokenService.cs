using BasicAuthDemo.Entities;

namespace BasicAuthDemo;

public interface ITokenService
{
    Task<string> CreateTokenAsync(UserEntity user);
}
