﻿namespace EA.Weee.Web.Tests.Unit.Areas.AatfReturn.Controller
{
    using System;
    using System.Collections.Generic;
    using System.Web.Mvc;
    using EA.Prsd.Core.Mapper;
    using EA.Weee.Api.Client;
    using EA.Weee.Core.AatfReturn;
    using EA.Weee.Core.Scheme;
    using EA.Weee.Requests.AatfReturn;
    using EA.Weee.Web.Areas.AatfReturn.Controllers;
    using EA.Weee.Web.Areas.AatfReturn.ViewModels;
    using EA.Weee.Web.Constant;
    using EA.Weee.Web.Controllers.Base;
    using EA.Weee.Web.Services;
    using EA.Weee.Web.Services.Caching;
    using FakeItEasy;
    using FluentAssertions;
    using Xunit;

    public class ReceivedPCSListControllerTests
    {
        private readonly IWeeeClient weeeClient;
        private readonly ReceivedPCSListController controller;
        private readonly BreadcrumbService breadcrumb;
        private readonly IMapper mapper;

        public ReceivedPCSListControllerTests()
        {
            weeeClient = A.Fake<IWeeeClient>();
            breadcrumb = A.Fake<BreadcrumbService>();
            mapper = A.Fake<IMapper>();
            controller = new ReceivedPCSListController(() => weeeClient, A.Fake<IWeeeCache>(), breadcrumb, mapper);
        }

        [Fact]
        public void CheckReceivedPCSListControllerInheritsExternalSiteController()
        {
            typeof(ReceivedPCSListController).BaseType.Name.Should().Be(typeof(ExternalSiteController).Name);
        }

        [Fact]
        public async void IndexGet_GivenValidViewModel_BreadcrumbShouldBeSet()
        {
            var organisationId = Guid.NewGuid();

            await controller.Index(organisationId, A.Dummy<Guid>());

            Assert.Equal(breadcrumb.ExternalActivity, BreadCrumbConstant.AatfReturn);
        }

        [Fact]
        public async void IndexGet_GivenActionExecutes_DefaultViewShouldBeReturned()
        {
            var result = await controller.Index(A.Dummy<Guid>(), A.Dummy<Guid>()) as ViewResult;

            result.ViewName.Should().BeEmpty();
        }

        [Fact]
        public async void IndexGet_GivenReturn_ApiShouldBeCalledWithReturnRequest()
        {
            var returnId = Guid.NewGuid();

            await controller.Index(Guid.NewGuid(), returnId);

            A.CallTo(() => weeeClient.SendAsync(A<string>._, A<GetReturnScheme>.That.Matches(g => g.ReturnId.Equals(returnId))))
            .MustHaveHappened(Repeated.Exactly.Once);
        }

        [Fact]
        public async void IndexGet_GivenActionAndParameters_ReceivedPCSListViewModelShouldBeReturned()
        {
            var returnData = new List<SchemeData>();
            var model = A.Fake<ReceivedPCSListViewModel>();
            var result = await controller.Index(A.Dummy<Guid>(), A.Dummy<Guid>()) as ViewResult;

            result.Model.Should().BeEquivalentTo(model);
        }

        [Fact]
        public async void IndexPost_OnSubmit_PageRedirectsToAatfTaskList()
        {
            var model = new ReceivedPCSListViewModel();
            var returnId = new Guid();
            var result = await controller.Index(model) as RedirectToRouteResult;

            result.RouteValues["action"].Should().Be("Index");
            result.RouteValues["controller"].Should().Be("AatfTaskList");
            result.RouteValues["area"].Should().Be("AatfReturn");
            result.RouteValues["returnId"].Should().Be(returnId);
        }
    }
}
