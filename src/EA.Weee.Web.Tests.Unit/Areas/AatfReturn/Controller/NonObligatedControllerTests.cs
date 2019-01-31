﻿namespace EA.Weee.RequestHandlers.Tests.Unit.AatfReturn
{
    using EA.Prsd.Core.Mapper;
    using EA.Prsd.Core.Web.OAuth;
    using EA.Weee.Api.Client;
    using EA.Weee.Web.Areas.AatfReturn.Requests;
    using EA.Weee.Web.Services;
        using EA.Weee.Web.Services.Caching;
    using FakeItEasy;
    using FluentAssertions;
    using Microsoft.Owin.Security;
    using Requests.AatfReturn;
    using System;
    using System.Security;
    using System.Security.Principal;
    using System.Threading.Tasks;
    using Requests.AatfReturn.NonObligated;
    using Web.Areas.AatfReturn.Controllers;
    using Web.Areas.AatfReturn.ViewModels;
    using Web.Controllers.Base;
    using Xunit;

    public class NonObligatedControllerTests
    {
        private readonly IWeeeClient weeeClient;
        private readonly INonObligatedWeeRequestCreator requestCreator;
        private readonly NonObligatedController controller;
        private readonly BreadcrumbService breadcrumb;

        public NonObligatedControllerTests()
        {
            weeeClient = A.Fake<IWeeeClient>();
            requestCreator = A.Fake<INonObligatedWeeRequestCreator>();
            breadcrumb = A.Fake<BreadcrumbService>();
            controller = new NonObligatedController(A.Fake<IWeeeCache>(), breadcrumb, () => weeeClient, requestCreator);
        }

        [Fact]
        public void CheckNonObligatedControllerInheritsExternalSiteController()
        {
            typeof(NonObligatedController).BaseType.Name.Should().Be(typeof(ExternalSiteController).Name);
        }

        [Fact]
        public async void Index_GivenValidViewModel_ApiSendShouldBeCalled()
        {
            var model = new NonObligatedValuesViewModel();
            var request = new AddNonObligatedRequest();

            A.CallTo(() => requestCreator.ViewModelToRequest(model)).Returns(request);

            await controller.Index(model);

            A.CallTo(() => weeeClient.SendAsync(A<string>.Ignored, request)).MustHaveHappened(Repeated.Exactly.Once);
        }

        [Fact]
        public async void Index_GivenValidViewModel_BreadcrumbShouldBeSet()
        {
            var organisationId = Guid.NewGuid();

            await controller.Index(organisationId, false);

            Assert.Equal(breadcrumb.ExternalActivity, "AATF Return");
        }
    }
}
