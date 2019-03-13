﻿namespace EA.Weee.Web.Areas.AatfReturn.Mappings.ToViewModel
{
    using EA.Weee.Api.Client;
    using EA.Weee.Core.AatfReturn;
    using EA.Weee.Requests.Shared;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web;

    public class ReturnAndAatfToSentOnCreateSiteViewModelMapTransfer
    {
        public Guid OrganisationId;

        public Guid ReturnId;

        public Guid AatfId;

        public AddressData SiteAddressData;

        public AddressData OperatorAddressData;
        
        public ReturnAndAatfToSentOnCreateSiteViewModelMapTransfer(IList<Core.Shared.CountryData> countryData)
        {
            this.SiteAddressData = new AddressData();
            SiteAddressData.Countries = countryData;
            this.OperatorAddressData = new AddressData();
        }
    }
}