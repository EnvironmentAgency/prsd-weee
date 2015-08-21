﻿namespace EA.Weee.Web.Tests.Unit.Areas.Scheme.Controllers
{
    using Api.Client;
    using Core.Organisations;
    using EA.Weee.Core.Shared;
    using EA.Weee.Requests.Scheme;
    using EA.Weee.Web.Services;
    using EA.Weee.Web.Services.Caching;
    using FakeItEasy;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Threading.Tasks;
    using System.Web.Mvc;
    using TestHelpers;
    using Web.Areas.Scheme.Controllers;
    using Web.Areas.Scheme.ViewModels;
    using Web.ViewModels.Shared;
    using Weee.Requests.Organisations;
    using Weee.Requests.Users;
    using Xunit;

    public class HomeControllerTests
    {
        private readonly IWeeeClient weeeClient;

        public HomeControllerTests()
        {
            weeeClient = A.Fake<IWeeeClient>();
        }

        [Fact]
        public async void GetChooseActivity_ChecksForValidityOfOrganisation()
        {
            try
            {
                await HomeController().ChooseActivity(A<Guid>._);
            }
            catch (Exception)
            {
            }

            A.CallTo(() => weeeClient.SendAsync(A<string>._, A<VerifyOrganisationExists>._))
                .MustHaveHappened(Repeated.Exactly.Once);
        }

        [Fact]
        public async void GetChooseActivity_IdDoesNotBelongToAnExistingOrganisation_ThrowsException()
        {
            A.CallTo(() => weeeClient.SendAsync(A<string>._, A<VerifyOrganisationExists>._))
                .Returns(false);

            await Assert.ThrowsAnyAsync<Exception>(() => HomeController().ChooseActivity(A<Guid>._));
        }

        [Fact]
        public async void GetChooseActivity_IdDoesBelongToAnExistingOrganisation_ReturnsView()
        {
            A.CallTo(() => weeeClient.SendAsync(A<string>._, A<VerifyOrganisationExists>._))
                .Returns(true);

            var result = await HomeController().ChooseActivity(A<Guid>._);

            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public async void GetChooseActivity_DoNotHaveOrganisationUser_ReturnsViewWithOnlyOneOption()
        {
            A.CallTo(() => weeeClient.SendAsync(A<string>._, A<VerifyOrganisationExists>._))
               .Returns(true);

            A.CallTo(() => weeeClient.SendAsync(A<string>._, A<GetUsersByOrganisationId>._))
               .Returns(new List<OrganisationUserData>());

            var result = await HomeController().ChooseActivity(A<Guid>._);

            var model = (ChooseActivityViewModel)((ViewResult)result).Model;

            Assert.Equal(model.ActivityOptions.PossibleValues.Count, 1);

            Assert.False(model.ActivityOptions.PossibleValues.Contains(PcsAction.ManageOrganisationUsers));

            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public async void GetChooseActivity_HaveOrganisationUser_ReturnsViewWithTwoOption()
        {
            A.CallTo(() => weeeClient.SendAsync(A<string>._, A<VerifyOrganisationExists>._))
               .Returns(true);

            A.CallTo(() => weeeClient.SendAsync(A<string>._, A<GetUsersByOrganisationId>._))
               .Returns(new List<OrganisationUserData>
               {
                   new OrganisationUserData
                   {
                       UserId = Guid.NewGuid().ToString()
                   },
                   new OrganisationUserData
                   {
                       UserId = Guid.NewGuid().ToString()
                   }
               });

            var result = await HomeController().ChooseActivity(A<Guid>._);

            var model = (ChooseActivityViewModel)((ViewResult)result).Model;

            Assert.Equal(model.ActivityOptions.PossibleValues.Count, 2);

            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public async void PostChooseActivity_ManagePcsMembersApprovedStatus_RedirectsToMemberRegistrationSummary()
        {
            A.CallTo(() => weeeClient.SendAsync(A<string>._, A<GetSchemeStatus>._)).Returns(SchemeStatus.Approved);

            var result = await HomeController().ChooseActivity(new ChooseActivityViewModel
            {
                ActivityOptions = new RadioButtonStringCollectionViewModel
                {
                    SelectedValue = PcsAction.ManagePcsMembers
                }
            });

            Assert.IsType<RedirectToRouteResult>(result);

            var routeValues = ((RedirectToRouteResult)result).RouteValues;

            Assert.Equal("Summary", routeValues["action"]);
            Assert.Equal("MemberRegistration", routeValues["controller"]);
        }

        [Fact]
        public async void PostChooseActivity_ManagePcsMembersPendingStatus_RedirectsToAuthorisationRequired()
        {
            A.CallTo(() => weeeClient.SendAsync(A<string>._, A<GetSchemeStatus>._)).Returns(SchemeStatus.Pending);

            var result = await HomeController().ChooseActivity(new ChooseActivityViewModel
            {
                ActivityOptions = new RadioButtonStringCollectionViewModel
                {
                    SelectedValue = PcsAction.ManagePcsMembers
                }
            });

            Assert.IsType<RedirectToRouteResult>(result);

            var routeValues = ((RedirectToRouteResult)result).RouteValues;

            Assert.Equal("AuthorizationRequired", routeValues["action"]);
            Assert.Equal("MemberRegistration", routeValues["controller"]);
        }

        [Fact]
        public async void PostChooseActivity_ManagePcsMembersRejectedStatus_RedirectsToAuthorisationRequired()
        {
            A.CallTo(() => weeeClient.SendAsync(A<string>._, A<GetSchemeStatus>._)).Returns(SchemeStatus.Rejected);

            var result = await HomeController().ChooseActivity(new ChooseActivityViewModel
            {
                ActivityOptions = new RadioButtonStringCollectionViewModel
                {
                    SelectedValue = PcsAction.ManagePcsMembers
                }
            });

            Assert.IsType<RedirectToRouteResult>(result);

            var routeValues = ((RedirectToRouteResult)result).RouteValues;

            Assert.Equal("AuthorizationRequired", routeValues["action"]);
            Assert.Equal("MemberRegistration", routeValues["controller"]);
        }

        [Fact]
        public async void PostChooseActivity_ManageOrganisationUsers_RedirectsToOrganisationUsersManagement()
        {
            var result = await HomeController().ChooseActivity(new ChooseActivityViewModel
            {
                ActivityOptions = new RadioButtonStringCollectionViewModel
                {
                    SelectedValue = PcsAction.ManageOrganisationUsers
                }
            });

            Assert.IsType<RedirectToRouteResult>(result);

            var routeValues = ((RedirectToRouteResult)result).RouteValues;

            Assert.Equal("ManageOrganisationUsers", routeValues["action"]);
        }

        [Fact]
        public async void GetManageOrganisationUsers_IdDoesNotBelongToAnExistingOrganisation_ThrowsException()
        {
            A.CallTo(() => weeeClient.SendAsync(A<string>._, A<VerifyOrganisationExists>._))
                .Returns(false);

            await Assert.ThrowsAnyAsync<Exception>(() => HomeController().ManageOrganisationUsers(A<Guid>._));
        }

        [Fact]
        public async void GetManageOrganisationUsers_IdDoesBelongToAnExistingOrganisation_ReturnsView()
        {
            A.CallTo(() => weeeClient.SendAsync(A<string>._, A<VerifyOrganisationExists>._))
                .Returns(true);

            A.CallTo(() => weeeClient.SendAsync(A<string>._, A<GetUsersByOrganisationId>._))
                .Returns(new List<OrganisationUserData>
                {
                    new OrganisationUserData(),
                });

            var result = await HomeController().ManageOrganisationUsers(A<Guid>._);

            Assert.IsType<ViewResult>(result);
            Assert.Equal(((ViewResult)result).ViewName, "ManageOrganisationUsers");
        }

        [Fact]
        public async void PostManageOrganisationUsers_ModalStateValid_ReturnsView()
        {
            var model = new OrganisationUsersViewModel
            {
                OrganisationUsers =
                    new StringGuidRadioButtons
                    {
                        PossibleValues =
                            new[] { new RadioButtonPair<string, Guid>("User (UserStatus)", Guid.NewGuid()), },
                        Selected = new RadioButtonPair<string, Guid>("User (UserStatus)", Guid.NewGuid()),
                        SelectedValue = Guid.NewGuid()
                    }
            };

            var context = new ValidationContext(model, null, null);
            var results = new List<ValidationResult>();
            var isModelStateValid = Validator.TryValidateObject(model, context, results, true);

            var result = await HomeController().ManageOrganisationUsers(A<Guid>._, model);

            Assert.IsType<ViewResult>(result);
            Assert.True(isModelStateValid);
        }

        private HomeController HomeController()
        {
            var controller = new HomeController(() => weeeClient, A.Fake<IWeeeCache>(), A.Fake<BreadcrumbService>());
            new HttpContextMocker().AttachToController(controller);

            return controller;
        }
    }
}
