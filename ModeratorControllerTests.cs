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

namespace FilmTests.Controllers
{
    [TestClass]
    public class ModeratorControllerTests
    {
        private Mock<DbContextOptions<ApplicationDbContext>> dbContextShota;
        private Mock<ApplicationDbContext> applicationDbContext;
        private ModeratorController controller;

        public ModeratorControllerTests()
        {
            var dbContextShota = new DbContextOptions<ApplicationDbContext>();
            applicationDbContext = new Mock<ApplicationDbContext>(dbContextShota);
            controller = new ModeratorController(applicationDbContext.Object);
        }

        [TestMethod]
        public void DeleteCommentConfirmed_CommentExists_Deleted()
        {
            var comment1 = new Comment
            {
                Id = 1,
                Body = "To Delete"
            };

            applicationDbContext.Setup(x => x.Comments.Find(1)).Returns(comment1);
            var result = controller.DeleteCommentConfirmed(1);
            Assert.IsNotInstanceOfType(result, typeof(NotFoundResult));
        }

        [TestMethod]
        public void DeleteCommentConfirmed_CommentDoesntExist_NotFound()
        {
            applicationDbContext.Setup(x => x.Comments.Find(2)).Returns<ApplicationDbContext, Comment>(null);
            var result = controller.DeleteCommentConfirmed(2);
            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        }

        [TestMethod]
        public void DeleteConfirmed_FilmExists_Deleted()
        {
            var movie1 = new Movie
            {
                Id = 1,
                Name = "Name1",
                Description = "Description1"
            };
   
            applicationDbContext.Setup(x => x.Films.Find(1)).Returns(movie1);
            
            var result = controller.DeleteConfirmed(1);

            Assert.IsNotInstanceOfType(result, typeof(NotFoundResult));
        }

        [TestMethod]
        public void DeleteConfirmed_FilmDoesntExist_NotFound()
        {
           
            applicationDbContext.Setup(x => x.Films.Find(2)).Returns<ApplicationDbContext, Movie>(null);

            var result = controller.DeleteConfirmed(2);

            Assert.IsInstanceOfType(result, typeof (NotFoundResult));
        }
    }
}

