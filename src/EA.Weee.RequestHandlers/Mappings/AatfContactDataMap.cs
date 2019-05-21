﻿namespace EA.Weee.RequestHandlers.Mappings
{
    using EA.Prsd.Core.Mapper;
    using EA.Weee.Core.AatfReturn;
    using EA.Weee.DataAccess;
    using EA.Weee.Domain;
    using EA.Weee.Domain.AatfReturn;
    using System.Linq;

    public class AatfContactDataMap : IMap<AatfContactData, AatfContact>
    {
        public AatfContact Map(AatfContactData source)
        {
            if (source != null)
            {
                return new AatfContact(source.FirstName, source.LastName, source.Position, source.AddressData.Address1, source.AddressData.Address2, source.AddressData.TownOrCity, source.AddressData.CountyOrRegion, source.AddressData.Postcode, source.AddressData.CountryId, source.Telephone, source.Email);
            }
            else
            {
                return new AatfContact();
            }
        }
    }
}