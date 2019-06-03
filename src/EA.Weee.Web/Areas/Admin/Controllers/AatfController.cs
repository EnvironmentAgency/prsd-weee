﻿namespace EA.Weee.Web.Areas.Admin.Controllers
{
    using EA.Prsd.Core.Domain;
    using EA.Prsd.Core.Extensions;
    using EA.Prsd.Core.Mapper;
    using EA.Weee.Api.Client;
    using EA.Weee.Core.AatfReturn;
    using EA.Weee.Requests.AatfReturn;
    using EA.Weee.Requests.AatfReturn.Internal;
    using EA.Weee.Requests.Admin;
    using EA.Weee.Requests.Shared;
    using EA.Weee.Security;
    using EA.Weee.Web.Areas.Admin.Controllers.Base;
    using EA.Weee.Web.Areas.Admin.Mappings.ToViewModel;
    using EA.Weee.Web.Areas.Admin.Requests;
    using EA.Weee.Web.Areas.Admin.ViewModels.Aatf;
    using EA.Weee.Web.Areas.Admin.ViewModels.Home;
    using EA.Weee.Web.Infrastructure;
    using EA.Weee.Web.Services;
    using EA.Weee.Web.Services.Caching;
    using System;
    using System.Collections.Generic;
    using System.Security.Claims;
    using System.Threading.Tasks;
    using System.Web.Mvc;

    public class AatfController : AdminController
    {
        private readonly Func<IWeeeClient> apiClient;
        private readonly BreadcrumbService breadcrumb;
        private readonly IMapper mapper;
        private readonly IEditAatfDetailsRequestCreator detailsRequestCreator;
        private readonly IEditAatfContactRequestCreator contactRequestCreator;
        private readonly IWeeeCache cache;

        public AatfController(Func<IWeeeClient> apiClient, BreadcrumbService breadcrumb, IMapper mapper, IEditAatfDetailsRequestCreator detailsRequestCreator, IEditAatfContactRequestCreator contactRequestCreator, IWeeeCache cache)
        {
            this.apiClient = apiClient;
            this.breadcrumb = breadcrumb;
            this.mapper = mapper;
            this.detailsRequestCreator = detailsRequestCreator;
            this.contactRequestCreator = contactRequestCreator;
            this.cache = cache;
        }

        [HttpGet]
        public async Task<ActionResult> Details(Guid id)
        {
            using (var client = apiClient())
            {
                var aatf = await client.SendAsync(User.GetAccessToken(), new GetAatfById(id));

                var associatedAatfs = await client.SendAsync(User.GetAccessToken(), new GetAatfsByOrganisationId(aatf.Organisation.Id));

                var associatedSchemes = await client.SendAsync(User.GetAccessToken(), new GetSchemesByOrganisationId(aatf.Organisation.Id));

                var viewModel = mapper.Map<AatfDetailsViewModel>(new AatfDataToAatfDetailsViewModelMapTransfer(aatf)
                {
                    OrganisationString = GenerateSharedAddress(aatf.Organisation.BusinessAddress),
                    SiteAddressString = GenerateAatfAddress(aatf.SiteAddress),
                    ContactAddressString = GenerateAatfAddress(aatf.Contact.AddressData), 
                    AssociatedAatfs = associatedAatfs,
                    AssociatedSchemes = associatedSchemes
                });

                SetBreadcrumb(aatf.FacilityType);

                return View(viewModel);
            }
        }

        [HttpGet]
        public async Task<ActionResult> ManageAatfs(FacilityType facilityType)
        {
            SetBreadcrumb(facilityType);

            return View(new ManageAatfsViewModel { FacilityType = facilityType, AatfDataList = await GetAatfs(facilityType), CanAddAatf = IsUserInternalAdmin(), Filter = new FilteringViewModel() { FacilityType = facilityType } });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ManageAatfs(ManageAatfsViewModel viewModel)
        {
            SetBreadcrumb(viewModel.FacilityType);

            if (!ModelState.IsValid)
            {
                using (var client = apiClient())
                {
                    viewModel = new ManageAatfsViewModel
                    {
                        AatfDataList = await GetAatfs(viewModel.FacilityType, viewModel.Filter),
                        CanAddAatf = IsUserInternalAdmin(),
                        FacilityType = viewModel.FacilityType,
                        Filter = viewModel.Filter
                    };
                    return View(viewModel);
                }
            }
            else
            {
                return RedirectToAction("Details", new { Id = viewModel.Selected.Value });
            }
        }

        private bool IsUserInternalAdmin()
        {
            ClaimsPrincipal claimsPrincipal = new ClaimsPrincipal(this.User);

            return claimsPrincipal.HasClaim(p => p.Value == Claims.InternalAdmin);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ApplyFilter(FilteringViewModel filter)
        {
            SetBreadcrumb(filter.FacilityType);
            return View(nameof(ManageAatfs), new ManageAatfsViewModel { AatfDataList = await GetAatfs(filter.FacilityType, filter), Filter = filter, FacilityType = filter.FacilityType });
        }

        [HttpGet]
        public async Task<ActionResult> ManageAatfDetails(Guid id)
        {
            using (var client = apiClient())
            {
                var aatf = await client.SendAsync(User.GetAccessToken(), new GetAatfById(id));

                if (!aatf.CanEdit)
                {
                    return new HttpForbiddenResult();
                }

                var viewModel = mapper.Map<AatfEditDetailsViewModel>(aatf);
                var accessToken = User.GetAccessToken();
                viewModel.CompetentAuthoritiesList = await client.SendAsync(accessToken, new GetUKCompetentAuthorities());
                viewModel.SiteAddress.Countries = await client.SendAsync(accessToken, new GetCountries(false));

                SetBreadcrumb(aatf.FacilityType);
                return View(viewModel);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ManageAatfDetails(AatfEditDetailsViewModel viewModel)
        {
            SetBreadcrumb(viewModel.FacilityType);

            if (ModelState.IsValid)
            {
                using (var client = apiClient())
                {
                    viewModel.CompetentAuthoritiesList = await client.SendAsync(User.GetAccessToken(), new GetUKCompetentAuthorities());
                    var request = detailsRequestCreator.ViewModelToRequest(viewModel);
                    await client.SendAsync(User.GetAccessToken(), request);

                    var aatf = await client.SendAsync(User.GetAccessToken(), new GetAatfById(viewModel.Id));

                    await cache.InvalidateAatfCache(aatf.Organisation.Id);
                }

                return Redirect(Url.Action("Details", new { area = "Admin", viewModel.Id }));
            }

            using (var client = apiClient())
            {
                var accessToken = User.GetAccessToken();
                viewModel.AatfStatusList = Enumeration.GetAll<AatfStatus>();
                viewModel.SizeList = Enumeration.GetAll<AatfSize>();
                viewModel.CompetentAuthoritiesList = await client.SendAsync(accessToken, new GetUKCompetentAuthorities());
                viewModel.SiteAddress.Countries = await client.SendAsync(accessToken, new GetCountries(false));
            }

            return View(viewModel);
        }

        [HttpGet]
        public async Task<ActionResult> ManageContactDetails(Guid id, FacilityType facilityType)
        {
            using (var client = apiClient())
            {
                var contact = await client.SendAsync(User.GetAccessToken(), new GetAatfContact(id));

                if (!contact.CanEditContactDetails)
                {
                    return new HttpForbiddenResult();
                }

                var viewModel = new AatfEditContactAddressViewModel()
                {
                    AatfId = id,
                    ContactData = contact,
                    FacilityType = facilityType
                };

                viewModel.ContactData.AddressData.Countries = await client.SendAsync(User.GetAccessToken(), new GetCountries(false));
                SetBreadcrumb(facilityType);
                return View(viewModel);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ManageContactDetails(AatfEditContactAddressViewModel viewModel)
        {
            SetBreadcrumb(viewModel.FacilityType);

            if (ModelState.IsValid)
            {
                using (var client = apiClient())
                {
                    var request = contactRequestCreator.ViewModelToRequest(viewModel);

                    await client.SendAsync(User.GetAccessToken(), request);

                    return Redirect(Url.Action("Details", new { area = "Admin", Id = viewModel.AatfId }) + "#contactDetails");
                }
            }

            using (var client = apiClient())
            {
                viewModel.ContactData.AddressData.Countries = await client.SendAsync(User.GetAccessToken(), new GetCountries(false));
            }

            return View(viewModel);
        }

        private async Task<List<AatfDataList>> GetAatfs(FacilityType facilityType, FilteringViewModel filter = null)
        {
            using (var client = apiClient())
            {
                var mappedFilter = filter != null ? mapper.Map<AatfFilter>(filter) : null;
                return await client.SendAsync(User.GetAccessToken(), new GetAatfs(facilityType, mappedFilter));
            }
        }

        public virtual string GenerateSharedAddress(Core.Shared.AddressData address)
        {
            var siteAddressLong = address.Address1;

            if (address.Address2 != null)
            {
                siteAddressLong += "<br/>" + address.Address2;
            }

            siteAddressLong += "<br/>" + address.TownOrCity;

            if (address.CountyOrRegion != null)
            {
                siteAddressLong += "<br/>" + address.CountyOrRegion;
            }

            if (address.Postcode != null)
            {
                siteAddressLong += "<br/>" + address.Postcode;
            }

            siteAddressLong += "<br/>" + address.CountryName;

            return siteAddressLong;
        }

        public virtual string GenerateAatfAddress(AddressData address)
        {
            var siteAddressLong = address.Address1;

            if (address.Address2 != null)
            {
                siteAddressLong += "<br/>" + address.Address2;
            }

            siteAddressLong += "<br/>" + address.TownOrCity;

            if (address.CountyOrRegion != null)
            {
                siteAddressLong += "<br/>" + address.CountyOrRegion;
            }

            if (address.Postcode != null)
            {
                siteAddressLong += "<br/>" + address.Postcode;
            }

            siteAddressLong += "<br/>" + address.CountryName;

            return siteAddressLong;
        }

        private void SetBreadcrumb(FacilityType type)
        {
            switch (type)
            {
                case FacilityType.Aatf:
                    breadcrumb.InternalActivity = InternalUserActivity.ManageAatfs;
                    break;
                case FacilityType.Ae:
                    breadcrumb.InternalActivity = InternalUserActivity.ManageAes;
                    break;
                default:
                    break;
            }
        }
    }
}