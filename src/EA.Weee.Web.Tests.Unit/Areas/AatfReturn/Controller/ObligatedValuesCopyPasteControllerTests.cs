﻿namespace EA.Weee.Web.Tests.Unit.Areas.AatfReturn.Controller
{
    using System;
    using System.Web.Mvc;
    using EA.Weee.Api.Client;
    using EA.Weee.Core.AatfReturn;
    using EA.Weee.Core.Scheme;
    using EA.Weee.Core.Shared;
    using EA.Weee.Requests.AatfReturn;
    using EA.Weee.Web.Areas.AatfReturn.Controllers;
    using EA.Weee.Web.Areas.AatfReturn.ViewModels;
    using EA.Weee.Web.Constant;
    using EA.Weee.Web.Services;
    using EA.Weee.Web.Services.Caching;
    using EA.Weee.Web.Tests.Unit.TestHelpers;
    using FakeItEasy;
    using FluentAssertions;
    using Xunit;

    public class ObligatedValuesCopyPasteControllerTests
    {
        private readonly ObligatedValuesCopyPasteController controller;
        private readonly BreadcrumbService breadcrumb;
        private readonly IWeeeCache cache;
        private readonly IWeeeClient weeeClient;
        private readonly IPasteProcessor pasteProcessor;

        public ObligatedValuesCopyPasteControllerTests()
        {
            breadcrumb = A.Fake<BreadcrumbService>();
            cache = A.Fake<IWeeeCache>();
            weeeClient = weeeClient = A.Fake<IWeeeClient>();
            pasteProcessor = A.Fake<IPasteProcessor>();

            controller = new ObligatedValuesCopyPasteController(() => weeeClient, breadcrumb, cache, pasteProcessor);
        }

        [Fact]
        public void CheckObligatedValueCopyPasteControllerInheritsAatfReturnBaseController()
        {
            typeof(ObligatedValuesCopyPasteController).BaseType.Name.Should().Be(typeof(AatfReturnBaseController).Name);
        }

        [Fact]
        public async void IndexGet_GivenActionExecutes_ApiShouldBeCalled()
        {
            var returnId = Guid.NewGuid();

            await controller.Index(returnId, A.Dummy<Guid>(), A.Dummy<Guid>());

            A.CallTo(() => weeeClient.SendAsync(A<string>._, A<GetReturn>.That.Matches(r => r.ReturnId.Equals(returnId))))
                .MustHaveHappened(Repeated.Exactly.Once);
        }

        [Fact]
        public async void IndexGet_GivenReturn_ViewModelShouldBeBuilt()
        {
            var returnId = Guid.NewGuid();
            var aatfId = Guid.NewGuid();
            var schemeId = Guid.NewGuid();
            var organisationId = Guid.NewGuid();
            var @return = A.Fake<ReturnData>();

            A.CallTo(() => @return.ReturnOperatorData.OrganisationId).Returns(organisationId);
            A.CallTo(() => weeeClient.SendAsync(A<string>._, A<GetReturn>.That.Matches(r => r.ReturnId.Equals(returnId)))).Returns(@return);

            var result = await controller.Index(returnId, aatfId, schemeId) as ViewResult;

            var viewModel = result.Model as ObligatedValuesCopyPasteViewModel;
            viewModel.AatfId.Should().Be(aatfId);
            viewModel.OrganisationId.Should().Be(organisationId);
            viewModel.ReturnId.Should().Be(returnId);
            viewModel.SchemeId.Should().Be(schemeId);
        }

        [Fact]
        public async void IndexGet_GivenValidViewModel_BreadcrumbShouldBeSet()
        {
            var organisationId = Guid.NewGuid();
            var @return = A.Fake<ReturnData>();
            var schemeInfo = A.Fake<SchemePublicInfo>();
            var operatorData = A.Fake<OperatorData>();
            const string orgName = "orgName";

            A.CallTo(() => weeeClient.SendAsync(A<string>._, A<GetReturn>._)).Returns(@return);
            A.CallTo(() => operatorData.OrganisationId).Returns(organisationId);
            A.CallTo(() => @return.ReturnOperatorData).Returns(operatorData);
            A.CallTo(() => cache.FetchOrganisationName(organisationId)).Returns(orgName);
            A.CallTo(() => cache.FetchSchemePublicInfo(organisationId)).Returns(schemeInfo);

            await controller.Index(A.Dummy<Guid>(), A.Dummy<Guid>(), A.Dummy<Guid>());

            breadcrumb.ExternalActivity.Should().Be(BreadCrumbConstant.AatfReturn);
            breadcrumb.ExternalOrganisation.Should().Be(orgName);
            breadcrumb.SchemeInfo.Should().Be(schemeInfo);
        }

        [Fact]
        public async void IndexPost_OnSubmit_PageRedirectsToObligatedReceived()
        {
            var httpContext = new HttpContextMocker();
            httpContext.AttachToController(controller);

            var schemeId = Guid.NewGuid();
            var returnId = Guid.NewGuid();
            var aatfId = Guid.NewGuid();

            var viewModel = new ObligatedValuesCopyPasteViewModel();
            viewModel.SchemeId = schemeId;
            viewModel.ReturnId = returnId;
            viewModel.AatfId = aatfId;
            viewModel.B2bPastedValues = new String[1];
            viewModel.B2cPastedValues = new String[1];

            httpContext.RouteData.Values.Add("schemeId", schemeId);
            httpContext.RouteData.Values.Add("returnId", returnId);
            httpContext.RouteData.Values.Add("aatfId", aatfId);

            var result = await controller.Index(viewModel, null) as RedirectToRouteResult;

            result.RouteValues["action"].Should().Be("Index");
            result.RouteValues["controller"].Should().Be("ObligatedReceived");
            result.RouteValues["area"].Should().Be("AatfReturn");
            result.RouteValues["schemeId"].Should().Be(schemeId);
            result.RouteValues["returnId"].Should().Be(returnId);
            result.RouteValues["aatfId"].Should().Be(aatfId);
        }

        [Fact]
        public async void IndexPost_OnSubmitWithBothPastedValues_TempDataShouldBeAttached()
        {
            var httpContext = new HttpContextMocker();
            httpContext.AttachToController(controller);

            var schemeId = Guid.NewGuid();
            var returnId = Guid.NewGuid();
            var aatfId = Guid.NewGuid();
            var pastedValues = new String[1] { "2\n" };

            var viewModel = new ObligatedValuesCopyPasteViewModel();
            viewModel.SchemeId = schemeId;
            viewModel.ReturnId = returnId;
            viewModel.AatfId = aatfId;
            viewModel.B2bPastedValues = pastedValues;
            viewModel.B2cPastedValues = pastedValues;
            
            var result = await controller.Index(viewModel, null) as RedirectToRouteResult;

            controller.TempData["pastedValues"].Should().NotBeNull();
        }

        [Fact]
        public async void IndexPost_OnSubmitWithOnePastedValues_TempDataShouldBeAttached()
        {
            var httpContext = new HttpContextMocker();
            httpContext.AttachToController(controller);

            var schemeId = Guid.NewGuid();
            var returnId = Guid.NewGuid();
            var aatfId = Guid.NewGuid();

            var viewModel = new ObligatedValuesCopyPasteViewModel();
            viewModel.SchemeId = schemeId;
            viewModel.ReturnId = returnId;
            viewModel.AatfId = aatfId;
            viewModel.B2bPastedValues = new String[1] { "2\n" };
            viewModel.B2cPastedValues = new String[1];

            var result = await controller.Index(viewModel, null) as RedirectToRouteResult;

            controller.TempData["pastedValues"].Should().NotBeNull();
        }

        [Fact]
        public async void IndexPost_OnSubmitWithNoPastedValues_TempDataShouldNotBeAttached()
        {
            var httpContext = new HttpContextMocker();
            httpContext.AttachToController(controller);

            var schemeId = Guid.NewGuid();
            var returnId = Guid.NewGuid();
            var aatfId = Guid.NewGuid();

            var viewModel = new ObligatedValuesCopyPasteViewModel();
            viewModel.SchemeId = schemeId;
            viewModel.ReturnId = returnId;
            viewModel.AatfId = aatfId;
            viewModel.B2bPastedValues = new String[1];
            viewModel.B2cPastedValues = new String[1];

            var result = await controller.Index(viewModel, null) as RedirectToRouteResult;

            controller.TempData["pastedValues"].Should().BeNull();
        }
    }
}