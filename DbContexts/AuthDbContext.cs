using BasicAuthDemo.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace BasicAuthDemo.DbContexts;

public class AuthDbContext : IdentityDbContext<UserEntity, UserRole, string>
{
    public AuthDbContext(DbContextOptions<AuthDbContext> options) : base(options)
    { }

}
