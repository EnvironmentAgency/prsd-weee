﻿namespace EA.Weee.Web.Tests.Unit.Areas.AatfReturn.Controller
{
    using EA.Prsd.Core.Mapper;
    using EA.Weee.Api.Client;
    using EA.Weee.Core.AatfReturn;
    using EA.Weee.Requests.AatfReturn;
    using EA.Weee.Requests.AatfReturn.Obligated;
    using EA.Weee.Web.Areas.AatfReturn.Controllers;
    using EA.Weee.Web.Areas.AatfReturn.Mappings.ToViewModel;
    using EA.Weee.Web.Areas.AatfReturn.Requests;
    using EA.Weee.Web.Areas.AatfReturn.ViewModels;
    using EA.Weee.Web.Constant;
    using EA.Weee.Web.Controllers.Base;
    using EA.Weee.Web.Services;
    using EA.Weee.Web.Services.Caching;
    using FakeItEasy;
    using FluentAssertions;
    using System;
    using System.Collections.Generic;
    using System.Web.Mvc;
    using Xunit;

    public class SentOnCreateSiteOperatorControllerTests
    {
        private readonly IWeeeClient apiClient;
        private readonly BreadcrumbService breadcrumb;
        private readonly IWeeeCache cache;
        private readonly IGetSentOnAatfSiteRequestCreator getRequestCreator;
        private readonly IEditSentOnAatfSiteRequestCreator requestCreator;
        private readonly SentOnCreateSiteOperatorController controller;
        private readonly IMap<ReturnAndAatfToSentOnCreateSiteOperatorViewModelMapTransfer, SentOnCreateSiteOperatorViewModel> mapper;

        public SentOnCreateSiteOperatorControllerTests()
        {
            this.apiClient = A.Fake<IWeeeClient>();
            this.breadcrumb = A.Fake<BreadcrumbService>();
            this.cache = A.Fake<IWeeeCache>();
            this.getRequestCreator = A.Fake<IGetSentOnAatfSiteRequestCreator>();
            this.requestCreator = A.Fake<IEditSentOnAatfSiteRequestCreator>();
            this.mapper = A.Fake<IMap<ReturnAndAatfToSentOnCreateSiteOperatorViewModelMapTransfer, SentOnCreateSiteOperatorViewModel>>();

            controller = new SentOnCreateSiteOperatorController(() => apiClient, breadcrumb, cache, requestCreator, mapper, getRequestCreator);
        }

        [Fact]
        public void CheckSentOnCreateSiteOperatorControllerInheritsExternalSiteController()
        {
            typeof(SentOnCreateSiteOperatorController).BaseType.Name.Should().Be(typeof(ExternalSiteController).Name);
        }

        [Fact]
        public async void IndexGet_GivenValidViewModel_BreadcrumbShouldBeSet()
        {
            var organisationId = Guid.NewGuid();
            var @return = A.Fake<ReturnData>();
            var operatorData = A.Fake<OperatorData>();
            const string orgName = "orgName";

            A.CallTo(() => apiClient.SendAsync(A<string>._, A<GetReturn>._)).Returns(@return);
            A.CallTo(() => operatorData.OrganisationId).Returns(organisationId);
            A.CallTo(() => @return.ReturnOperatorData).Returns(operatorData);
            A.CallTo(() => cache.FetchOrganisationName(organisationId)).Returns(orgName);

            await controller.Index(@return.Id, organisationId, Guid.NewGuid(), Guid.NewGuid(), null);

            breadcrumb.ExternalActivity.Should().Be(BreadCrumbConstant.AatfReturn);
            breadcrumb.ExternalOrganisation.Should().Be(orgName);
        }

        [Fact]
        public async void IndexGet_GivenAction_DefaultViewShouldBeReturned()
        {
            var result = await controller.Index(A.Dummy<Guid>(), A.Dummy<Guid>(), A.Dummy<Guid>(), A.Dummy<Guid>(), null) as ViewResult;

            result.ViewName.Should().BeEmpty();
        }

        [Fact]
        public async void IndexGet_GivenWeeeSentOnId_ApiShouldBeCalled()
        {
            var aatfId = Guid.NewGuid();
            var returnId = Guid.NewGuid();
            var weeeSentOnId = Guid.NewGuid();
            var weeeSentOnList = new List<WeeeSentOnData>();
            var weeeSentOn = new WeeeSentOnData();
            weeeSentOn.SiteAddress = new AatfAddressData();
            weeeSentOn.SiteAddressId = Guid.NewGuid();
            weeeSentOn.WeeeSentOnId = weeeSentOnId;
            weeeSentOnList.Add(weeeSentOn);

            A.CallTo(() => apiClient.SendAsync(A<string>._, A<GetWeeeSentOn>._)).Returns(weeeSentOnList);

            await controller.Index(returnId, A.Dummy<Guid>(), aatfId, weeeSentOnId, null);

            A.CallTo(() => apiClient.SendAsync(A<string>._, A<GetWeeeSentOn>.That.Matches(w => w.AatfId == aatfId && w.ReturnId == returnId && w.WeeeSentOnId == weeeSentOnId))).MustHaveHappened(Repeated.Exactly.Once);
        }

        [Fact]
        public async void IndexPost_GivenInvalidViewModel_ApiShouldNotBeCalled()
        {
            var form = new FormCollection();
            controller.ModelState.AddModelError("error", "error");
            var model = new SentOnCreateSiteOperatorViewModel();
            model.OperatorAddressData = new OperatorAddressData("TEST", "TEST", "TEST", "TEST", "TEST", "TEST", Guid.NewGuid(), "TEST");
            await controller.Index(model, form);

            A.CallTo(() => apiClient.SendAsync(A<string>._, A<EditSentOnAatfSiteWithOperator>._)).MustNotHaveHappened();
        }

        [Fact]
        public async void IndexPost_GivenValidViewModel_ApiSendShouldBeCalled()
        {
            var form = new FormCollection();
            var model = new SentOnCreateSiteOperatorViewModel();
            model.OrganisationId = Guid.NewGuid();
            model.AatfId = Guid.NewGuid();
            model.WeeeSentOnId = Guid.NewGuid();
            model.ReturnId = Guid.NewGuid();
            model.SiteAddressData = new AatfAddressData("TEST", "TEST", "TEST", "TEST", "TEST", "TEST", Guid.NewGuid(), "TEST");
            var request = new EditSentOnAatfSite();

            A.CallTo(() => requestCreator.ViewModelToRequest(model)).Returns(request);

            await controller.Index(model, form);

            A.CallTo(() => apiClient.SendAsync(A<string>._, request)).MustHaveHappened(Repeated.Exactly.Once);
        }

        [Theory]
        [InlineData("true")]
        [InlineData("false")]
        public async void IndexPost_GivenValidViewModel_IsOperatorTheSameAsAATFShouldBeSet(string operatorBool)
        {
            var form = new FormCollection();
            var boolConversion = Convert.ToBoolean(operatorBool);
            var model = new SentOnCreateSiteOperatorViewModel();
            model.OrganisationId = Guid.NewGuid();
            model.AatfId = Guid.NewGuid();
            model.WeeeSentOnId = Guid.NewGuid();
            model.ReturnId = Guid.NewGuid();
            model.OperatorAddressData = new OperatorAddressData("TEST", "TEST", "TEST", "TEST", "TEST", "TEST", Guid.NewGuid(), "TEST");
            model.SiteAddressData = new AatfAddressData("TEST", "TEST", "TEST", "TEST", "TEST", "TEST", Guid.NewGuid(), "TEST");

            form.Add("IsOperatorTheSameAsAATF", operatorBool);

            await controller.Index(model, form);

            A.CallTo(() => requestCreator.ViewModelToRequest(A<SentOnCreateSiteOperatorViewModel>.That.Matches(m => m.IsOperatorTheSameAsAATF == boolConversion))).MustHaveHappened(Repeated.Exactly.Once);
        }
    }
}
