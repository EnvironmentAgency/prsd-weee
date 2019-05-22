﻿namespace EA.Weee.Web.Areas.AatfReturn.Controllers
{
    using EA.Prsd.Core.Mapper;
    using EA.Weee.Api.Client;
    using EA.Weee.Core.AatfReturn;
    using EA.Weee.Requests.AatfReturn.Obligated;
    using EA.Weee.Requests.Shared;
    using EA.Weee.Web.Areas.AatfReturn.Mappings.ToViewModel;
    using EA.Weee.Web.Areas.AatfReturn.Requests;
    using EA.Weee.Web.Areas.AatfReturn.ViewModels;
    using EA.Weee.Web.Constant;
    using EA.Weee.Web.Controllers.Base;
    using EA.Weee.Web.Infrastructure;
    using EA.Weee.Web.Services;
    using EA.Weee.Web.Services.Caching;
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Web.Mvc;
    using Attributes;

    [ValidateReturnCreatedActionFilter]
    public class SentOnCreateSiteOperatorController : ExternalSiteController
    {
        private readonly Func<IWeeeClient> apiClient;
        private readonly BreadcrumbService breadcrumb;
        private readonly IWeeeCache cache;
        private readonly IEditSentOnAatfSiteRequestCreator requestCreator;
        private readonly IGetSentOnAatfSiteRequestCreator getRequestCreator;
        private readonly IMap<ReturnAndAatfToSentOnCreateSiteOperatorViewModelMapTransfer, SentOnCreateSiteOperatorViewModel> mapper;

        public SentOnCreateSiteOperatorController(Func<IWeeeClient> apiClient, BreadcrumbService breadcrumb, IWeeeCache cache, IEditSentOnAatfSiteRequestCreator requestCreator, IMap<ReturnAndAatfToSentOnCreateSiteOperatorViewModelMapTransfer, SentOnCreateSiteOperatorViewModel> mapper, IGetSentOnAatfSiteRequestCreator getRequestCreator)
        {
            this.apiClient = apiClient;
            this.breadcrumb = breadcrumb;
            this.cache = cache;
            this.requestCreator = requestCreator;
            this.getRequestCreator = getRequestCreator;
            this.mapper = mapper;
        }

        [HttpGet]
        public virtual async Task<ActionResult> Index(Guid returnId, Guid organisationId, Guid aatfId, Guid weeeSentOnId, bool? javascriptDisabled)
        {
            using (var client = apiClient())
            {
                var operatorCountryData = await client.SendAsync(User.GetAccessToken(), new GetCountries(false));

                var request = new GetSentOnAatfSite(weeeSentOnId);

                var siteAddressData = await client.SendAsync(User.GetAccessToken(), request);

                var operatorAddress = new AatfAddressData();
                Guid? operatorAddressId = null;

                var weeeSentOnList = await client.SendAsync(User.GetAccessToken(), new GetWeeeSentOn(aatfId, returnId, weeeSentOnId));
                if (weeeSentOnList.Count > 0)
                {
                    var weeeSentOn = weeeSentOnList[0];
                    operatorAddress = weeeSentOn.OperatorAddress;
                    operatorAddressId = weeeSentOn.OperatorAddressId;
                }

                var viewModel = mapper.Map(new ReturnAndAatfToSentOnCreateSiteOperatorViewModelMapTransfer() { ReturnId = returnId, SiteAddressData = siteAddressData, OperatorAddressId = operatorAddressId, AatfId = aatfId, OrganisationId = organisationId, WeeeSentOnId = weeeSentOnId, JavascriptDisabled = javascriptDisabled, CountryData = operatorCountryData, OperatorAddressData = operatorAddress });

                await SetBreadcrumb(organisationId, BreadCrumbConstant.AatfReturn);
                
                return View(viewModel);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public virtual async Task<ActionResult> Index(SentOnCreateSiteOperatorViewModel viewModel, FormCollection formCollection)
        {
            var isOperatorTheSame = Convert.ToBoolean(formCollection["IsOperatorTheSameAsAATF"]);
            viewModel.IsOperatorTheSameAsAATF = isOperatorTheSame;

            if (ModelState.IsValid)
            {
                using (var client = apiClient())
                {
                    var request = requestCreator.ViewModelToRequest(viewModel);

                    await client.SendAsync(User.GetAccessToken(), request);
                    return AatfRedirect.ObligatedSentOn(viewModel.SiteAddressData.Name, viewModel.OrganisationId, viewModel.AatfId, viewModel.ReturnId, (Guid)viewModel.WeeeSentOnId);
                }
            }

            using (var client = apiClient())
            {
                viewModel.OperatorAddressData.Countries = await client.SendAsync(User.GetAccessToken(), new GetCountries(false));
            }

            await SetBreadcrumb(viewModel.OrganisationId, BreadCrumbConstant.AatfReturn);
            return View(viewModel);
        }

        private async Task SetBreadcrumb(Guid organisationId, string activity)
        {
            breadcrumb.ExternalOrganisation = await cache.FetchOrganisationName(organisationId);
            breadcrumb.ExternalActivity = activity;
            breadcrumb.SchemeInfo = await cache.FetchSchemePublicInfo(organisationId);
        }
    }
}