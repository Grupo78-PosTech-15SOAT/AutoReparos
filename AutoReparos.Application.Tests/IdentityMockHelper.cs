using AutoReparos.Domain.Usuarios.Entities;
using Microsoft.AspNetCore.Identity;
using NSubstitute;

namespace AutoReparos.Application.Tests
{
    public static class IdentityMockHelper
    {
        public static UserManager<TUser> MockUserManager<TUser>() where TUser : class
        {
            var store = Substitute.For<IUserStore<TUser>>();
            var userManager = Substitute.For<UserManager<TUser>>(store, null!, null!, null!, null!, null!, null!, null!, null!);
            return userManager;
        }
    }
}
