﻿namespace EA.Weee.Web.Areas.AatfReturn.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using System.Web.Mvc;
    using Api.Client;
    using Core.AatfReturn;
    using EA.Weee.Requests.AatfReturn;
    using Infrastructure;
    using Microsoft.Owin.Security;
    using Prsd.Core.Mapper;
    using Prsd.Core.Web.OAuth;
    using Requests;
    using Services;
    using Services.Caching;
    using ViewModels;
    using Web.Controllers.Base;
    using Weee.Requests.AatfReturn.NonObligated;

    public class CheckYourReturnController : Controller
    {
        private readonly Func<IWeeeClient> apiClient;
        private readonly IWeeeCache cache;
        private readonly BreadcrumbService breadcrumb;
        public decimal? TonnageTotal = 0.000m;
        public decimal? TonnageDcfTotal = 0.000m;

        public CheckYourReturnController(Func<IWeeeClient> apiClient,
            IWeeeCache cache,
            BreadcrumbService breadcrumb)
        {
            this.apiClient = apiClient;
            this.cache = cache;
            this.breadcrumb = breadcrumb;
        }

        [HttpGet]
        public virtual async Task<ActionResult> Index(Guid returnId, Guid organisationId)
        {
            List<decimal?> tonnageList;
            List<decimal?> tonnageDcfList;
            ReturnData @return;

            await SetBreadcrumb(organisationId, "AATF Return");

            using (var client = apiClient())
            {
                @return = await client.SendAsync(User.GetAccessToken(), new GetReturn(returnId));
                tonnageList = await client.SendAsync(User.GetAccessToken(), new FetchNonObligatedWeeeForReturnRequest(returnId, organisationId, false));
                tonnageDcfList = await client.SendAsync(User.GetAccessToken(), new FetchNonObligatedWeeeForReturnRequest(returnId, organisationId, true));
            }

            CalculateListTotal(tonnageList, false);
            CalculateListTotal(tonnageDcfList, true);

            var viewModel = new CheckYourReturnViewModel(TonnageTotal, TonnageDcfTotal, @return.Quarter, @return.QuarterWindow, @return.Quarter.Year);
            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public virtual async Task<ActionResult> Index(CheckYourReturnViewModel viewModel)
        {
            return await Task.Run<ActionResult>(() => 
                RedirectToAction("Index", "SubmittedReturn", new { area  = "AatfReturn", organisationId = RouteData.Values["organisationId"], returnId = RouteData.Values["returnId"] }));
        }

        private void CalculateListTotal(List<decimal?> list, bool dcf)
        {
            if (dcf)
            {
                foreach (var number in list)
                {
                    if (number != null)
                    {
                        TonnageDcfTotal += number;
                    }
                }
            }
            else
            {
                foreach (var number in list)
                {
                    if (number != null)
                    {
                        TonnageTotal += number;
                    }
                }
            }
        }

        private async Task SetBreadcrumb(Guid organisationId, string activity)
        {
            breadcrumb.ExternalOrganisation = await cache.FetchOrganisationName(organisationId);
            breadcrumb.ExternalActivity = activity;
            breadcrumb.SchemeInfo = await cache.FetchSchemePublicInfo(organisationId);
        }
    }
}
