﻿namespace EA.Weee.Web.Tests.Unit.Areas.Admin.Controllers
{
    using System.Threading.Tasks;
    using System.Web.Mvc;
    using Api.Client;
    using Core.Search;
    using Core.Shared;
    using FakeItEasy;
    using Services;
    using Services.Caching;
    using TestHelpers;
    using ViewModels.Shared.Submission;
    using Web.Areas.Admin.Controllers;
    using Web.Areas.Scheme.ViewModels;
    using Xunit;

    public class SubmissionsControllerTests
    {
        private readonly IWeeeClient weeeClient;

        public SubmissionsControllerTests()
        {
            weeeClient = A.Fake<IWeeeClient>();
        }
        
        [Fact]
        public async void GetChooseSubmissionType_ReturnsView()
        {
            var result = await SubmissionsController().ChooseSubmissionType();

            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public async void PostChooseSubmissionType_ModelIsInvalid_ShouldRedirectViewWithModel()
        {
            var controller = SubmissionsController();
            controller.ModelState.AddModelError("Key", "Any error");

            var model = new ChooseSubmissionTypeViewModel
            {
                SelectedValue = SubmissionType.EeeOrWeeeDataReturns
            };
            var result = await controller.ChooseSubmissionType(model);

            Assert.IsType<ViewResult>(result);
            Assert.Equal(model, ((ViewResult)(result)).Model);
            Assert.False(controller.ModelState.IsValid);
        }

        [Fact]
        public async void PostChooseSubmissionType_MemberRegistrationsSelected_RedirectsToMemberRegistrationSubmissionHistory()
        {
            var result = await SubmissionsController().ChooseSubmissionType(new ChooseSubmissionTypeViewModel
            {
                SelectedValue = SubmissionType.MemberRegistrations
            });

            Assert.IsType<RedirectToRouteResult>(result);

            var routeValues = ((RedirectToRouteResult)result).RouteValues;

            Assert.Equal("SubmissionsHistory", routeValues["action"]);
        }

        [Fact]
        public async void PostChooseSubmissionType_DataReturnsSelected_RedirectsToDataReturnsSubmissionHistory()
        {
            var result = await SubmissionsController().ChooseSubmissionType(new ChooseSubmissionTypeViewModel
            {
                SelectedValue = SubmissionType.EeeOrWeeeDataReturns
            });

            Assert.IsType<RedirectToRouteResult>(result);

            var routeValues = ((RedirectToRouteResult)result).RouteValues;

            Assert.Equal("DataReturnSubmissionHistory", routeValues["action"]);
        }

        private SubmissionsController SubmissionsController()
        {
            ConfigurationService configService = A.Fake<ConfigurationService>();
            var controller = new SubmissionsController(A.Fake<BreadcrumbService>(), () => weeeClient, A.Fake<IWeeeCache>(), A.Fake<CsvWriterFactory>());
            new HttpContextMocker().AttachToController(controller);
            return controller;
        }
    }
}
