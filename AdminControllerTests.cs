using Film.Controllers;
using Film.Data;
using Film.Models;
using FilmTests.Stubs;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FilmTests.Mocks;
using FilmTests.Stubs;
using System;

namespace FilmTests.Controllers
{
    [TestClass]
    public class AdminControllerTests
    {
        private Mock<DbContextOptions<ApplicationDbContext>> dbContextShota;
        private Mock<ApplicationDbContext> applicationDbContext;
        private Mock<UserManager<ApplicationUser>> userManagerMock;
        private Mock<RoleManager<IdentityRole>> roleManagerMock;
        private AdminController controller;


        public static UserManager<ApplicationUser> CreateUserManager(IUserStore<ApplicationUser> store = null)
        {
            store = store ?? new Mock<IUserStore<ApplicationUser>>().Object;
            var options = new Mock<IOptions<IdentityOptions>>();
            var idOptions = new IdentityOptions();
            idOptions.Lockout.AllowedForNewUsers = false;
            options.Setup(o => o.Value).Returns(idOptions);
            var userValidators = new List<IUserValidator<ApplicationUser>>();
            var validator = new Mock<IUserValidator<ApplicationUser>>();
            userValidators.Add(validator.Object);
            var pwdValidators = new List<PasswordValidator<ApplicationUser>>();
            pwdValidators.Add(new PasswordValidator<ApplicationUser>());
            var userManager = new UserManagerStub(store, options.Object, new PasswordHasher<ApplicationUser>(),
                userValidators, pwdValidators, new UpperInvariantLookupNormalizer(),
                new IdentityErrorDescriber(), null,
                new Mock<ILogger<UserManager<ApplicationUser>>>().Object);
            validator.Setup(v => v.ValidateAsync(userManager, It.IsAny<ApplicationUser>()))
                .Returns(Task.FromResult(IdentityResult.Success)).Verifiable();

            return userManager;
        }

        private Mock<UserManager<ApplicationUser>> CreateUserManagerMock()
        {
            var mockUserManager = new Mock<UserManager<ApplicationUser>>(
                    new Mock<IUserStore<ApplicationUser>>().Object,
                    new Mock<IOptions<IdentityOptions>>().Object,
                    new Mock<IPasswordHasher<ApplicationUser>>().Object,
                    new IUserValidator<ApplicationUser>[0],
                    new IPasswordValidator<ApplicationUser>[0],
                    new Mock<ILookupNormalizer>().Object,
                    new Mock<IdentityErrorDescriber>().Object,
                    new Mock<IServiceProvider>().Object,
                    new Mock<ILogger<UserManager<ApplicationUser>>>().Object);

            return mockUserManager;
        }

        private Mock<RoleManager<IdentityRole>> CreateRoleManagerMock()
        {
            var mockRoleManager = new Mock<RoleManager<IdentityRole>>(
                    new Mock<IRoleStore<IdentityRole>>().Object,
                    new IRoleValidator<IdentityRole>[0],
                    new Mock<ILookupNormalizer>().Object,
                    new Mock<IdentityErrorDescriber>().Object,
                    new Mock<ILogger<RoleManager<IdentityRole>>>().Object);

            return mockRoleManager;
        }

        public AdminControllerTests()
        {
            var appDbContext = new Mock<ApplicationDbContext>();
            var dbContextShota = new DbContextOptions<ApplicationDbContext>();
            applicationDbContext = new Mock<ApplicationDbContext>(dbContextShota);
            roleManagerMock = CreateRoleManagerMock();
            userManagerMock = CreateUserManagerMock();
            controller = new AdminController(applicationDbContext.Object, userManagerMock.Object, roleManagerMock.Object);
        }

      
        [TestMethod]
        public void Ban_Unblocked_Blocked()
        {
            const string userId = "1";
            var user = new ApplicationUser
            {
                Id = userId,
                Blocked = false
            };
            userManagerMock.Setup(a => a.FindByIdAsync(userId)).ReturnsAsync(user);
            controller.Ban("1");
            Assert.AreEqual(true, user.Blocked);
        }

        [TestMethod]
        public void Ban_Blocked_Unblocked()
        {
            const string userId = "1";
            var user = new ApplicationUser
            {
                Id = userId,
                Blocked = true
            };
            userManagerMock.Setup(a => a.FindByIdAsync(userId)).ReturnsAsync(user);
            controller.Ban("1");
            Assert.AreEqual(false, user.Blocked);
        }

        [TestMethod]
        public void PromoteToRole_RoleIsUser_RoleSetToAdmin()
        {
            const string roleId = "admin";
            const string userId = "1";
            roleManagerMock.Setup(p => p.FindByIdAsync(roleId)).ReturnsAsync(new IdentityRole
            {
                Id = roleId,
                Name = "Admin Account"
            });
            var user = new ApplicationUser
            {
                Id = userId,
                Email = "somemail@gmail.com"
            };
            userManagerMock.Setup(a => a.FindByIdAsync(userId)).ReturnsAsync(user);
            controller.PromoteToRole(userId, roleId);
            roleManagerMock.Verify(a => a.FindByIdAsync(roleId));
            userManagerMock.Verify(a => a.FindByIdAsync(userId));
            userManagerMock.Verify(a => a.AddToRoleAsync(user, roleId));
        }
    }
}
