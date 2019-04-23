﻿namespace EA.Weee.RequestHandlers.AatfReturn.ObligatedSentOn
{
    using EA.Weee.Core.AatfReturn;
    using EA.Weee.DataAccess;
    using EA.Weee.Domain;
    using EA.Weee.Domain.AatfReturn;
    using EA.Weee.RequestHandlers.AatfReturn.ObligatedReused;
    using EA.Weee.RequestHandlers.Organisations;
    using EA.Weee.RequestHandlers.Security;
    using EA.Weee.Requests.AatfReturn.Obligated;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public class EditSentOnAatfSiteHandler
    {
        private readonly WeeeContext context;
        private readonly IReturnDataAccess returnDataAccess;
        private readonly IWeeeAuthorization authorization;
        private readonly ISentOnAatfSiteDataAccess sentOnDataAccess;
        private readonly IAatfSiteDataAccess offSiteDataAccess;
        private readonly IGenericDataAccess genericDataAccess;
        private readonly IOrganisationDetailsDataAccess organisationDetailsDataAccess;

        public EditSentOnAatfSiteHandler(WeeeContext context, IWeeeAuthorization authorization,
        ISentOnAatfSiteDataAccess sentOnDataAccess, IGenericDataAccess genericDataAccess, IReturnDataAccess returnDataAccess, IOrganisationDetailsDataAccess orgDataAccess, IAatfSiteDataAccess offSiteDataAccess)
        {
            this.context = context;
            this.authorization = authorization;
            this.sentOnDataAccess = sentOnDataAccess;
            this.genericDataAccess = genericDataAccess;
            this.returnDataAccess = returnDataAccess;
            this.organisationDetailsDataAccess = orgDataAccess;
            this.offSiteDataAccess = offSiteDataAccess;
        }

        public async Task<Guid> HandleAsync(EditSentOnAatfSite message)
        {
            authorization.EnsureCanAccessExternalArea();

            Country country = await organisationDetailsDataAccess.FetchCountryAsync(message.SiteAddressData.CountryId);

            var value = await genericDataAccess.GetById<AatfAddress>(message.SiteAddressData.Id);

            var addressData = new SiteAddressData()
            {
                Name = message.SiteAddressData.Name,
                Address1 = message.SiteAddressData.Address1,
                Address2 = message.SiteAddressData.Address2,
                TownOrCity = message.SiteAddressData.TownOrCity,
                CountyOrRegion = message.SiteAddressData.CountyOrRegion,
                Postcode = message.SiteAddressData.Postcode,
                CountryName = message.SiteAddressData.CountryName,
                CountryId = message.SiteAddressData.CountryId
            };

            await offSiteDataAccess.Update(value, addressData, country);

            return addressData.Id;
        }
    }
}
