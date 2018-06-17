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
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using System;

namespace FilmTests.Controllers
{
    [TestClass]
    public class HomeControllerTests
    {
        private Mock<DbContextOptions<ApplicationDbContext>> dbContextShota;
        private Mock<ApplicationDbContext> applicationDbContext;
        private UserManager<ApplicationUser> userManager;
        private HomeController controller;

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

        public HomeControllerTests()
        {
            var dbContextShota = new DbContextOptions<ApplicationDbContext>();
            applicationDbContext = new Mock<ApplicationDbContext>(dbContextShota);

            userManager = CreateUserManager();
            controller = new HomeController(applicationDbContext.Object, userManager);
        }

        private IEnumerable<Movie> GetMoviesFixture()
        {
            return new List<Movie>
            {
                new Movie
                {
                   Id = 1,
                   Name = "Дюнкерк",
                   Director= "Крістофер Нолан",
                   Scenario = "Крістофер Нолан"
                },
                new Movie
                {
                   Id = 2,
                   Name = "Гра Моллі",
                   Director= "Аарон Соркін",
                   Scenario = "Аарон Соркін"
                }
            };
        }
        private IEnumerable<Category> GetCategoriesFixture()
        {
            return new List<Category>
            {
                new Category
                {
                    Id = 1,
                    Name = "Драма",
                },
                new Category
                {
                    Id = 2,
                    Name = "Біографічний",
                }
            };
        }

        [TestMethod]
        public void Filter_Has1Category_CategorySelected()
        {
            // Arrange
            ICollection<CategoryFilm> filmCategories = new List<CategoryFilm>
            {
                new CategoryFilm
                {
                    CategoryId = 1,
                    FilmId = 1
                }
            };

            var films = GetMoviesFixture();
            var categories = GetCategoriesFixture();

            filmCategories.First().Category = categories.First();
            filmCategories.First().Name = films.First();
            filmCategories.First().Film = films.First();

            films.First().Categories = filmCategories;
            categories.First().Films = filmCategories;

            applicationDbContext.Setup(x => x.Films).ReturnsDbSet(films);
            applicationDbContext.Setup(x => x.Categories).ReturnsDbSet(categories);

            // Act
            var result = ((List<Movie>)(((ViewResult)controller.Index(
                selectedCategories: new string[] { "Драма" })).Model));

            //Assert
            Xunit.Assert.Equal(1, result.Count);
        }

        [TestMethod]
        public void Filter_Has2Categories_CategoriesSelected()
        {
            // Arrange
            ICollection<CategoryFilm> filmCategories = new List<CategoryFilm>
            {
                new CategoryFilm
                {
                    CategoryId = 1,
                    FilmId = 2
                },

                new CategoryFilm
                {
                    CategoryId = 2,
                    FilmId = 2
                }
            };

            var films = GetMoviesFixture();

            var categories = GetCategoriesFixture();
             
            filmCategories.First().Category = categories.First();
            filmCategories.Last().Category = categories.Last();
            filmCategories.First().Name = films.First();
            filmCategories.First().Film = films.First();
            filmCategories.Last().Name = films.First();
            filmCategories.Last().Film = films.First();

            films.First().Categories = filmCategories;
            categories.First().Films = filmCategories;
            categories.Last().Films = filmCategories;

            applicationDbContext.Setup(x => x.Films).ReturnsDbSet(films);
            applicationDbContext.Setup(x => x.Categories).ReturnsDbSet(categories);

            // Act
            var result = ((List<Movie>)(((ViewResult)controller.Index(
                selectedCategories: new string[] { "Драма", "Біографічний" })).Model));

            //Assert
            Xunit.Assert.Equal(1, result.Count);
            Xunit.Assert.Equal(2, result[0].Categories.Count);
        }
        [TestMethod]
        public void CreateComment_CommentExists_CommentCreated()
        {
            ICollection<CategoryFilm> filmCategories = new List<CategoryFilm>
            {
                new CategoryFilm
                {
                    CategoryId = 1,
                    FilmId = 1
                }
            };

            var films = GetMoviesFixture();

            var categories = GetCategoriesFixture();
            var comments = new List<Comment>();

            filmCategories.First().Category = categories.First();
            filmCategories.First().Name = films.First();
            filmCategories.First().Film = films.First();

            films.First().Categories = filmCategories;
            categories.First().Films = filmCategories;

            applicationDbContext.Setup(x => x.Films).ReturnsDbSet(films);
            applicationDbContext.Setup(x => x.Categories).ReturnsDbSet(categories);
            applicationDbContext.Setup(x => x.Comments).ReturnsDbSet(comments);

            var cp = new Mock<ClaimsPrincipal>();
            cp.Setup(m => m.HasClaim(It.IsAny<string>(), It.IsAny<string>())).Returns(true);
            cp.SetupGet(p => p.Identity.Name).Returns("Test UserName");

            var contextMock = new Mock<HttpContext>();
            contextMock.Setup(p => p.User).Returns(cp.Object);

            var controllerContextMock = new Mock<ControllerContext>();
            ((ControllerContext)(controllerContextMock.Object)).HttpContext = contextMock.Object;

            var tempData = new TempDataDictionary(contextMock.Object, Mock.Of<ITempDataProvider>());
            controller.TempData = tempData;
            controller.TempData.Add("FilmId", 1);
            controller.ControllerContext = controllerContextMock.Object;

            var comment = new Comment { Body = "description" };

            controller.CreateComment(comment);

            applicationDbContext.Verify(c => c.SaveChanges());
        }

        [TestMethod]
        public void Search_Name_FilmExists()
        {
            
            ICollection<CategoryFilm> filmCategories = new List<CategoryFilm>
            {
                new CategoryFilm
                {
                    CategoryId = 1,
                    FilmId = 1
                },
                new CategoryFilm
                {
                    CategoryId = 1,
                    FilmId = 2
                }
            };

            var films = GetMoviesFixture();

            var categories = GetCategoriesFixture();

            filmCategories.First().Category = categories.First();
            filmCategories.Last().Category = categories.First();
            filmCategories.First().Name = films.First();
            filmCategories.First().Film = films.First();
            filmCategories.Last().Name = films.Last();
            filmCategories.Last().Film = films.Last();

            films.First().Categories = filmCategories;
            films.Last().Categories = filmCategories;
            categories.First().Films = filmCategories;

            applicationDbContext.Setup(x => x.Films).ReturnsDbSet(films);
            applicationDbContext.Setup(x => x.Categories).ReturnsDbSet(categories);

            HomeController controller = new HomeController(applicationDbContext.Object, userManager);
            ViewResult result = (ViewResult)controller.Index("Дюнкерк", false, null);
            Assert.AreEqual(1, ((List<Movie>)result.Model).Count);
            Assert.AreEqual("Дюнкерк", ((List<Movie>)result.Model).First().Name);
        }

        [TestMethod]
        public void Search_Director_FilmExists()
        {
           
            ICollection<CategoryFilm> filmCategories = new List<CategoryFilm>
            {
                new CategoryFilm
                {
                    CategoryId = 1,
                    FilmId = 1
                },
                new CategoryFilm
                {
                    CategoryId = 1,
                    FilmId = 2
                }
            };

            var films = GetMoviesFixture();

            var categories = GetCategoriesFixture();

            filmCategories.First().Category = categories.First();
            filmCategories.Last().Category = categories.First();
            filmCategories.First().Name = films.First();
            filmCategories.First().Name = films.First();
            filmCategories.Last().Name = films.Last();
            filmCategories.Last().Film = films.Last();

            films.First().Categories = filmCategories;
            films.Last().Categories = filmCategories;
            categories.First().Films = filmCategories;

            applicationDbContext.Setup(x => x.Films).ReturnsDbSet(films);
            applicationDbContext.Setup(x => x.Categories).ReturnsDbSet(categories);

            HomeController controller = new HomeController(applicationDbContext.Object, userManager);
            ViewResult result = (ViewResult)controller.Index("Крістофер Нолан", false, null);
            Assert.AreEqual(1, ((List<Movie>)result.Model).Count);
            Assert.AreEqual("Крістофер Нолан", ((List<Movie>)result.Model).First().Director);
        }

        [TestMethod]
        public void Search_Screenwriter_FilmExists()
        {
            
            ICollection<CategoryFilm> filmCategories = new List<CategoryFilm>
            {
                new CategoryFilm
                {
                    CategoryId = 1,
                    FilmId = 1
                },
                new CategoryFilm
                {
                    CategoryId = 1,
                    FilmId = 2
                }
            };

            var films = GetMoviesFixture();

            var categories = GetCategoriesFixture();

            filmCategories.First().Category = categories.First();
            filmCategories.Last().Category = categories.First();
            filmCategories.First().Name = films.First();
            filmCategories.First().Name = films.First();
            filmCategories.Last().Name = films.Last();
            filmCategories.Last().Film = films.Last();

            films.First().Categories = filmCategories;
            films.Last().Categories = filmCategories;
            categories.First().Films = filmCategories;

            applicationDbContext.Setup(x => x.Films).ReturnsDbSet(films);
            applicationDbContext.Setup(x => x.Categories).ReturnsDbSet(categories);

            HomeController controller = new HomeController(applicationDbContext.Object, userManager);
            ViewResult result = (ViewResult)controller.Index("Крістофер Нолан", false, null);
            Assert.AreEqual(1, ((List<Movie>)result.Model).Count);
            Assert.AreEqual("Крістофер Нолан", ((List<Movie>)result.Model).First().Scenario);
        }

        [TestMethod]
        public void Search_FilmNameDoesntExist_WasNotFound()
        {
            ICollection<CategoryFilm> filmCategories = new List<CategoryFilm>
            {
                new CategoryFilm
                {
                    CategoryId = 1,
                    FilmId = 1
                },
                new CategoryFilm
                {
                    CategoryId = 1,
                    FilmId = 2
                }
            };

            var films = GetMoviesFixture();
            var categories = GetCategoriesFixture();

            filmCategories.First().Category = categories.First();
            filmCategories.Last().Category = categories.First();
            filmCategories.First().Name = films.First();
            filmCategories.First().Film = films.First();
            filmCategories.Last().Name = films.Last();
            filmCategories.Last().Film = films.Last();

            films.First().Categories = filmCategories;
            films.Last().Categories = filmCategories;
            categories.First().Films = filmCategories;

            applicationDbContext.Setup(x => x.Films).ReturnsDbSet(films);
            applicationDbContext.Setup(x => x.Categories).ReturnsDbSet(categories);

            HomeController controller = new HomeController(applicationDbContext.Object, userManager);
            ViewResult result = (ViewResult)controller.Index("Складки часу", false, null);
            //Assert.IsNull(((List<Movie>)result.Model).Count, 0);
            Assert.IsNull(((List<Movie>)result.Model).FirstOrDefault(x => x.Name == "Складки часу"));
        }
        [TestMethod]
        public void Search_FilmScreenwriterDoesntExist_WasNotFound()
        {
            ICollection<CategoryFilm> filmCategories = new List<CategoryFilm>
            {
                new CategoryFilm
                {
                    CategoryId = 1,
                    FilmId = 1
                },
                new CategoryFilm
                {
                    CategoryId = 1,
                    FilmId = 2
                }
            };

            var films = GetMoviesFixture();
            var categories = GetCategoriesFixture();

            filmCategories.First().Category = categories.First();
            filmCategories.Last().Category = categories.First();
            filmCategories.First().Name = films.First();
            filmCategories.First().Film = films.First();
            filmCategories.Last().Name = films.Last();
            filmCategories.Last().Film = films.Last();

            films.First().Categories = filmCategories;
            films.Last().Categories = filmCategories;
            categories.First().Films = filmCategories;

            applicationDbContext.Setup(x => x.Films).ReturnsDbSet(films);
            applicationDbContext.Setup(x => x.Categories).ReturnsDbSet(categories);

            HomeController controller = new HomeController(applicationDbContext.Object, userManager);
            ViewResult result = (ViewResult)controller.Index("Дженіфер Лі", false, null);
            Assert.IsNull(((List<Movie>)result.Model).FirstOrDefault(x => x.Name == "Дженіфер Лі"));
        }
        [TestMethod]
        public void Search_FilmDirectorDoesntExist_WasNotFound()
        {
            ICollection<CategoryFilm> filmCategories = new List<CategoryFilm>
            {
                new CategoryFilm
                {
                    CategoryId = 1,
                    FilmId = 1
                },
                new CategoryFilm
                {
                    CategoryId = 1,
                    FilmId = 2
                }
            };

            var films = GetMoviesFixture();
            var categories = GetCategoriesFixture();

            filmCategories.First().Category = categories.First();
            filmCategories.Last().Category = categories.First();
            filmCategories.First().Name = films.First();
            filmCategories.First().Film = films.First();
            filmCategories.Last().Name = films.Last();
            filmCategories.Last().Film = films.Last();

            films.First().Categories = filmCategories;
            films.Last().Categories = filmCategories;
            categories.First().Films = filmCategories;

            applicationDbContext.Setup(x => x.Films).ReturnsDbSet(films);
            applicationDbContext.Setup(x => x.Categories).ReturnsDbSet(categories);

            HomeController controller = new HomeController(applicationDbContext.Object, userManager);
            ViewResult result = (ViewResult)controller.Index("Ава Дюверней", false, null);
            Assert.IsNull(((List<Movie>)result.Model).FirstOrDefault(x => x.Name == "Ава Дюверней"));
        }

        [TestMethod]
        public void Search_FilmExists_NotCaseSensitive()
        {
            
            ICollection<CategoryFilm> filmCategories = new List<CategoryFilm>
            {
                new CategoryFilm
                {
                    CategoryId = 1,
                    FilmId = 1
                },
                new CategoryFilm
                {
                    CategoryId = 1,
                    FilmId = 2
                }
            };

            var films = GetMoviesFixture();

            var categories = GetCategoriesFixture();

            filmCategories.First().Category = categories.First();
            filmCategories.Last().Category = categories.First();
            filmCategories.First().Name = films.First();
            filmCategories.First().Film = films.First();
            filmCategories.Last().Name = films.Last();
            filmCategories.Last().Film = films.Last();

            films.First().Categories = filmCategories;
            films.Last().Categories = filmCategories;
            categories.First().Films = filmCategories;

            applicationDbContext.Setup(x => x.Films).ReturnsDbSet(films);
            applicationDbContext.Setup(x => x.Categories).ReturnsDbSet(categories);

            HomeController controller = new HomeController(applicationDbContext.Object, userManager);
            ViewResult result = (ViewResult)controller.Index("ДЮНКЕРК", false, null);
            Assert.AreEqual(1, ((List<Movie>)result.Model).Count);
            Assert.AreEqual("Дюнкерк", ((List<Movie>)result.Model).First().Name);
        }

        private IEnumerable<Mark> GetMarksFixture(IEnumerable<Movie> movies)
        {
            return new List<Mark>
            {
                new Mark
                {
                   Id = 1,
                   FilmId = 1,
                   Film = movies.First(),
                   MarkValue = 8,
                   UserId = "1",
                   UserName = "User1"
                },
                new Mark
                {
                   Id = 2,
                   FilmId = 2,
                   Film = movies.Last(),
                   MarkValue = 7,
                   UserId = "1",
                   UserName = "User1"
                }
            };
        }

        private HomeController BuildSUT()
        {
            var contextMock = new Mock<HttpContext>();

            var controllerContextMock = new Mock<ControllerContext>();
            ((ControllerContext)(controllerContextMock.Object)).HttpContext = contextMock.Object;

            var tempData = new TempDataDictionary(contextMock.Object, Mock.Of<ITempDataProvider>());
            return new HomeController(applicationDbContext.Object, userManager)
            {
                TempData = tempData
            };
        }

        [TestMethod]
        public void PutMark_MarkIsAlreadySaved_MarkIsChanged()
        {

            var films = GetMoviesFixture();
            var marks = GetMarksFixture(films);

            applicationDbContext.Setup(m => m.Marks).ReturnsDbSet(marks);

            
            var mark = marks.First();

            var sut = BuildSUT();
            sut.TempData.Add("FilmId", 1);

            var m1 = applicationDbContext.Object.Marks;
            sut.PutMark(mark);
            var m2 = applicationDbContext.Object.Marks;

            applicationDbContext.Verify(a => a.Marks.Add(It.IsAny<Mark>()), Times.Never);
            applicationDbContext.Verify(a => a.SaveChanges());
        }

        [TestMethod]
        public void PutMark_MarkIsNotInDb_MarkIsAdded()
        {
            var films = GetMoviesFixture();
            var marks = GetMarksFixture(films);
            var mark = marks.First();
            var expectedMarks = marks.Except(new Mark[] { mark });

            applicationDbContext.Setup(x => x.Films).ReturnsDbSet(films);
            applicationDbContext.Setup(m => m.Marks).ReturnsDbSet(expectedMarks);

            var sut = BuildSUT();
            sut.TempData.Add("FilmId", 1);

            sut.PutMark(mark);

            //applicationDbContext.Verify(a => a.Marks.Add(It.IsAny<Mark>()), Times.Exactly(2));
            //Assert.AreEqual(marks.Count(), expectedMarks.Count());
            applicationDbContext.Verify(a => a.Marks, Times.Exactly(2));         
            applicationDbContext.Verify(a => a.Films, Times.Once);         
            applicationDbContext.Verify(a => a.SaveChanges());
        }

        [TestMethod]
        public void PutMark_FilmIdEqualsZero_NoDbOperationPerformed()
        {

            var sut = BuildSUT();
            sut.TempData.Add("FilmId", 0);
            sut.PutMark(null);

            applicationDbContext.Verify(a => a.Marks.Add(It.IsAny<Mark>()), Times.Never);
            applicationDbContext.Verify(a => a.SaveChanges(), Times.Never);
        }
    }
}
