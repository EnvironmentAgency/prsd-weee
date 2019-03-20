﻿namespace EA.Weee.Core.AatfReturn
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using EA.Weee.Core.DataStandards;
    using EA.Weee.Core.Shared;

    public class SiteAddressData : AddressData
    {
        [Required]
        [StringLength(CommonMaxFieldLengths.DefaultString)]
        [Display(Name = "Site name")]
        public override string Name { get; set; }

        public SiteAddressData(string name, string address1, string address2, string townOrCity, string countyOrRegion, string postcode, Guid countryId, string countryName) 
            : base(name, address1, address2, townOrCity, countyOrRegion, postcode, countryId, countryName)
        {
        }

        public SiteAddressData()
        {
        }
    }
}