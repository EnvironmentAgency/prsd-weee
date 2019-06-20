﻿namespace EA.Weee.Web.Tests.Unit.Areas.Admin.Controllers
{
    using System;
    using System.Threading.Tasks;
    using System.Web.Mvc;
    using Api.Client;
    using Core.Admin;
    using Core.Shared;
    using EA.Prsd.Core.Mapper;
    using EA.Weee.Web.Infrastructure;
    using EA.Weee.Web.Services;
    using FakeItEasy;
    using Security;
    using Web.Areas.Admin.Controllers;
    using Web.Areas.Admin.ViewModels.User;
    using Weee.Requests.Admin;
    using Weee.Requests.Users;
    using Xunit;
    using GetUserData = Weee.Requests.Admin.GetUserData;

    public class UserControllerTests
    {
        private readonly Func<IWeeeClient> apiClient;
        private readonly IWeeeClient weeeClient;
        private readonly IMapper mapper;
        private readonly UserController controller;

        public UserControllerTests()
        {
            weeeClient = A.Fake<IWeeeClient>();
            apiClient = () => weeeClient;
            mapper = A.Fake<IMapper>();
            controller = new UserController(apiClient, A.Fake<BreadcrumbService>(), mapper);
        }

        [Fact]
        public async Task PostIndex_WithValidModel_RedirectsToViewAction()
        {
            // Arrange
            Guid selectedUserId = Guid.NewGuid();

            // Act
            ActionResult result = await controller.Index(new ManageUsersViewModel { SelectedUserId = selectedUserId });

            // Assert
            Assert.NotNull(result);
            Assert.IsType<RedirectToRouteResult>(result);

            var redirectValues = ((RedirectToRouteResult)result).RouteValues;
            Assert.Equal("View", redirectValues["action"]);
            Assert.Equal(selectedUserId, redirectValues["id"]);
        }

        [Fact]
        public async Task GetEdit_ReturnsEditView_WhenCanEditUserIsTrue()
        {
            A.CallTo(() => weeeClient.SendAsync(A<string>._, A<GetUserData>._))
                .Returns(new ManageUserData
                {
                    UserStatus = UserStatus.Active,
                    OrganisationId = Guid.NewGuid(),
                    Id = Guid.NewGuid(),
                    UserId = Guid.NewGuid().ToString(),
                    FirstName = "Firstname",
                    LastName = "Lastname",
                    Email = "test@ea.com",
                    IsCompetentAuthorityUser = true,
                    OrganisationName = "test ltd.",
                    CanEditUser = true
                });

            var result = await controller.Edit(Guid.NewGuid());

            var model = ((ViewResult)result).Model;

            A.CallTo(() => weeeClient.SendAsync(A<string>._, A<GetUserData>._))
                .MustHaveHappened(Repeated.Exactly.Once);
            A.CallTo(() => weeeClient.SendAsync(A<string>._, A<GetRoles>._))
                .MustHaveHappened(Repeated.Exactly.Once);

            Assert.NotNull(result);
            Assert.IsType<ViewResult>(result);
            Assert.IsType<EditUserViewModel>(model);
        }

        [Fact]
        public async Task GetEdit_ReturnsHttpForbiddenResult_WhenCanEditUserIsFalse()
        {
            A.CallTo(() => weeeClient.SendAsync(A<string>._, A<GetUserData>._))
                .Returns(new ManageUserData
                {
                    CanEditUser = false
                });

            var result = await controller.Edit(Guid.NewGuid());

            Assert.NotNull(result);
            Assert.IsType<HttpForbiddenResult>(result);
        }

        [Fact]
        public async Task PostEdit_WithCompetentAuthorityUserAndValidModel_UpdatesUserAndCompetentAuthorityUserRoleAndStatusAndRedirectsToViewAction()
        {
            var model = new EditUserViewModel
            {
                UserStatus = UserStatus.Active,
                OrganisationId = Guid.NewGuid(),
                UserId = Guid.NewGuid().ToString(),
                FirstName = "Firstname",
                LastName = "Lastname",
                Email = "test@ea.com",
                IsCompetentAuthorityUser = true,
                OrganisationName = "test ltd.",
                Role = new Role { Name = "InternalAdmin", Description = "Administrator" }
            };

            Guid id = Guid.NewGuid();
            var result = await controller.Edit(id, model);

            A.CallTo(() => weeeClient.SendAsync(A<string>._, A<UpdateUser>._))
                .MustHaveHappened(Repeated.Exactly.Once);

            A.CallTo(() => weeeClient.SendAsync(A<string>._, A<UpdateCompetentAuthorityUserRoleAndStatus>._))
                .MustHaveHappened(Repeated.Exactly.Once);

            Assert.NotNull(result);
            Assert.IsType<RedirectToRouteResult>(result);

            var redirectValues = ((RedirectToRouteResult)result).RouteValues;
            Assert.Equal("View", redirectValues["action"]);
            Assert.Equal(id, redirectValues["id"]);
        }

        [Fact]
        public async Task PostEdit_WithOrganisationUserAndValidModel_UpdatesUserAndOrganisationUserStatusAndRedirectsToViewAction()
        {
            var model = new EditUserViewModel
            {
                UserStatus = UserStatus.Active,
                OrganisationId = Guid.NewGuid(),
                UserId = Guid.NewGuid().ToString(),
                FirstName = "Firstname",
                LastName = "Lastname",
                Email = "test@ea.com",
                IsCompetentAuthorityUser = false,
                OrganisationName = "test ltd."
            };

            Guid id = Guid.NewGuid();
            var result = await controller.Edit(id, model);

            A.CallTo(() => weeeClient.SendAsync(A<string>._, A<UpdateUser>._))
                .MustHaveHappened(Repeated.Exactly.Once);

            A.CallTo(() => weeeClient.SendAsync(A<string>._, A<UpdateOrganisationUserStatus>._))
                .MustHaveHappened(Repeated.Exactly.Once);

            Assert.NotNull(result);
            Assert.IsType<RedirectToRouteResult>(result);

            var redirectValues = ((RedirectToRouteResult)result).RouteValues;
            Assert.Equal("View", redirectValues["action"]);
            Assert.Equal(id, redirectValues["id"]);
        }

        [Fact]
        public async Task PostEdit_WithCompetentAuthorityUserAndValidModelAndUserBeingUpdatedIsCurrentUser_UpdatesUserAndDoesNotUpdateCompetentAuthorityUserRoleAndStatusAndRedirectsToViewAction()
        {
            var model = new EditUserViewModel
            {
                UserStatus = UserStatus.Active,
                OrganisationId = Guid.NewGuid(),
                UserId = controller.User.GetUserId(),
                FirstName = "Firstname",
                LastName = "Lastname",
                Email = "test@ea.com",
                IsCompetentAuthorityUser = true,
                OrganisationName = "test ltd."
            };

            Guid id = Guid.NewGuid();
            var result = await controller.Edit(id, model);

            A.CallTo(() => weeeClient.SendAsync(A<string>._, A<UpdateUser>._))
                .MustHaveHappened(Repeated.Exactly.Once);

            A.CallTo(() => weeeClient.SendAsync(A<string>._, A<UpdateCompetentAuthorityUserRoleAndStatus>._))
                .MustNotHaveHappened();

            Assert.NotNull(result);
            Assert.IsType<RedirectToRouteResult>(result);

            var redirectValues = ((RedirectToRouteResult)result).RouteValues;
            Assert.Equal("View", redirectValues["action"]);
            Assert.Equal(id, redirectValues["id"]);
        }
    }
}
