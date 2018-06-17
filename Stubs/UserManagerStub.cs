using Film.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace FilmTests.Stubs
{
    public class UserManagerStub : UserManager<ApplicationUser>
    {
        public UserManagerStub(
            IUserStore<ApplicationUser> store,
            IOptions<IdentityOptions> optionsAccessor,
            IPasswordHasher<ApplicationUser> passwordHasher,
            IEnumerable<IUserValidator<ApplicationUser>> userValidators,
            IEnumerable<IPasswordValidator<ApplicationUser>> passwordValidators,
            ILookupNormalizer keyNormalizer,
            IdentityErrorDescriber errors,
            IServiceProvider services,
            ILogger<UserManager<ApplicationUser>> logger)
            : base(store, optionsAccessor, passwordHasher, userValidators, passwordValidators, keyNormalizer, errors, services, logger)
        {
        }

        public override string GetUserId(ClaimsPrincipal principal)
        {
            return "1";
        }

        public override Task<ApplicationUser> GetUserAsync(ClaimsPrincipal principal)
        {
            var user = new ApplicationUser { Id = "1", Email = "somemail@gmail.com" };
            return Task.FromResult(user);
        }

        public override Task<ApplicationUser> FindByIdAsync(string userId)
        {
            var user = new ApplicationUser { Id = "1", Email = "somemail@gmail.com" };
            return Task.FromResult(user);
        }

        public override Task<IdentityResult> AddToRoleAsync(ApplicationUser user, string role)
        {
            return base.AddToRoleAsync(user, role);
        }
    }
}
