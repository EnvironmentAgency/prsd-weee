﻿namespace EA.Weee.Web.Areas.AatfReturn.Controllers
{
    using System;
    using System.Threading.Tasks;
    using System.Web.Mvc;
    using EA.Weee.Api.Client;
    using EA.Weee.Core.AatfReturn;
    using EA.Weee.Web.Areas.AatfReturn.Requests;
    using EA.Weee.Web.Areas.AatfReturn.ViewModels;
    using EA.Weee.Web.Constant;
    using EA.Weee.Web.Controllers.Base;
    using EA.Weee.Web.Infrastructure;
    using EA.Weee.Web.Services;
    using EA.Weee.Web.Services.Caching;
    using Weee.Requests.AatfReturn;

    public class ObligatedReusedController : AatfReturnBaseController
    {
        private readonly Func<IWeeeClient> apiClient;
        private readonly IObligatedReusedWeeeRequestCreator requestCreator;
        private readonly BreadcrumbService breadcrumb;
        private readonly IWeeeCache cache;

        public ObligatedReusedController(IWeeeCache cache, BreadcrumbService breadcrumb, Func<IWeeeClient> apiClient, IObligatedReusedWeeeRequestCreator requestCreator)
        {
            this.apiClient = apiClient;
            this.requestCreator = requestCreator;
            this.breadcrumb = breadcrumb;
            this.cache = cache;
        }

        [HttpGet]
        public virtual async Task<ActionResult> Index(Guid returnId, Guid aatfId)
        {
            using (var client = apiClient())
            {
                var @return = await client.SendAsync(User.GetAccessToken(), new GetReturn(returnId));

                var viewModel = new ObligatedViewModel(new ObligatedCategoryValues())
                {
                    OrganisationId = @return.ReturnOperatorData.OrganisationId,
                    ReturnId = returnId,
                    AatfId = aatfId,
                    AatfName = (await cache.FetchAatfData(@return.ReturnOperatorData.OrganisationId, aatfId)).Name
                };

                await SetBreadcrumb(@return.ReturnOperatorData.OrganisationId, BreadCrumbConstant.AatfReturn);

                return View(viewModel);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public virtual async Task<ActionResult> Index(ObligatedViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                using (var client = apiClient())
                {
                    var request = requestCreator.ViewModelToRequest(viewModel);

                    await client.SendAsync(User.GetAccessToken(), request);

                    return AatfRedirect.ReusedOffSite(viewModel.ReturnId, viewModel.AatfId, viewModel.OrganisationId);
                }
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