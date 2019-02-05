﻿using EA.Prsd.Core.Mapper;
using EA.Weee.Api.Client;
using EA.Weee.Requests.AatfReturn;
using EA.Weee.Requests.Organisations;
using EA.Weee.Web.Areas.AatfReturn.ViewModels;
using EA.Weee.Web.Controllers.Base;
using EA.Weee.Web.Infrastructure;
using EA.Weee.Web.Services;
using EA.Weee.Web.Services.Caching;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace EA.Weee.Web.Areas.AatfReturn.Controllers
{
    public class AatfTaskListController : ExternalSiteController
    {
        private readonly Func<IWeeeClient> apiClient;
        private readonly BreadcrumbService breadcrumb;
        private readonly IWeeeCache cache;
        private readonly IMapper mapper;

        public AatfTaskListController(Func<IWeeeClient> apiClient, BreadcrumbService breadcrumb, IWeeeCache cache, IMapper mapper)
        {
            this.apiClient = apiClient;
            this.breadcrumb = breadcrumb;
            this.cache = cache;
            this.mapper = mapper;
        }

        [HttpGet]
        public virtual async Task<ActionResult> Index(Guid organisationId, Guid returnId)
        {
            var viewModel = new AatfTaskListViewModel() { OrganisationId = organisationId, ReturnId = returnId};
            using (var client = apiClient())
            {
                var organisationName = (await client.SendAsync(User.GetAccessToken(), new GetOrganisationInfo(organisationId))).OrganisationName;
                var @return = await client.SendAsync(User.GetAccessToken(), new GetReturn(returnId));
                var compliancePeriod = mapper.Map<SubmittedReturnViewModel>(@return);

                viewModel.OrganisationName = organisationName;

                viewModel.Year = compliancePeriod.Year;
                viewModel.Quarter = compliancePeriod.Quarter;
                viewModel.Period = compliancePeriod.Period;
            }

            var aatfs = new List<string>();
            aatfs.Add("ABB Ltd Darlaston");
            aatfs.Add("ABB Ltd Woking");
            aatfs.Add("ABB Ltd Maidenhead");

            viewModel.Aatfs = aatfs;

            await SetBreadcrumb(organisationId, "AATF Task List");
            return View(viewModel);
        }

        [HttpPost]
        public virtual async Task<ActionResult> Index(AatfTaskListViewModel viewModel)
        {
            return RedirectToAction("Index", "CheckYourReturn", new { area = "AatfReturn", returnid = viewModel.ReturnId, organisationid = viewModel.OrganisationId });
        }

        private async Task SetBreadcrumb(Guid organisationId, string activity)
        {
            breadcrumb.ExternalOrganisation = await cache.FetchOrganisationName(organisationId);
            breadcrumb.ExternalActivity = activity;
            breadcrumb.SchemeInfo = await cache.FetchSchemePublicInfo(organisationId);
        }
    }
}