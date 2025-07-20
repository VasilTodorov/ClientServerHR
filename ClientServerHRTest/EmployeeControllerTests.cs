using ClientServerHR.Controllers;
using ClientServerHR.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using System.Security.Claims;


namespace ClientServerHR.Tests
{
    public class EmployeeControllerTests
    {
        private Mock<IEmployeeRepository> _employeeRepoMock;
        private Mock<UserManager<ApplicationUser>> _userManagerMock;
        private Mock<ILogger<EmployeeController>> _loggerMock;

        public EmployeeControllerTests()
        {
            _employeeRepoMock = new Mock<IEmployeeRepository>();

            var store = new Mock<IUserStore<ApplicationUser>>();
            _userManagerMock = new Mock<UserManager<ApplicationUser>>(
                store.Object, null, null, null, null, null, null, null, null
            );

            _loggerMock = new Mock<ILogger<EmployeeController>>();
        }

        private EmployeeController CreateController(ClaimsPrincipal? user = null)
        {
            var controller = new EmployeeController(
                _employeeRepoMock.Object,
                _userManagerMock.Object,
                _loggerMock.Object
            );

            if (user != null)
            {
                controller.ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext { User = user }
                };
            }

            return controller;
        }


        [Fact]
        public void List_ReturnsNotFound_WhenUserIdIsNull()
        {
            // Arrange
            _userManagerMock.Setup(x => x.GetUserId(It.IsAny<ClaimsPrincipal>()))
                            .Returns((string?)null);
            var controller = CreateController();

            // Act
            var result = controller.List();

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public void Applicants_ReturnsViewResultWithApplicants()
        {
            // Arrange
            var applicants = new List<ApplicationUser>
            {
                new ApplicationUser { Id = "1", FirstName = "Test", Employee = null }
            };

            _userManagerMock.Setup(x => x.Users)
                            .Returns(applicants.AsQueryable());

            var controller = CreateController();

            // Act
            var result = controller.Applicants() as ViewResult;

            // Assert
            Assert.NotNull(result);
            var model = Assert.IsAssignableFrom<IEnumerable<ApplicationUser>>(result.Model);
            Assert.Single(model);
        }

        [Fact]
        public void HireComplete_ReturnsViewResult()
        {
            var controller = CreateController();
            var result = controller.HireComplete();
            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public void Index_RedirectsToMyProfile()
        {
            var controller = CreateController();
            var result = controller.Index() as RedirectToActionResult;
            Assert.NotNull(result);
            Assert.Equal("MyProfile", result.ActionName);
        }
    }
}
