﻿namespace EA.Weee.RequestHandlers.Tests.DataAccess.AatfReturn
{
    using System;
    using System.Data.Entity;
    using System.Linq;
    using System.Threading.Tasks;
    using EA.Weee.Core.Shared;
    using EA.Weee.Domain.AatfReturn;
    using EA.Weee.Domain.Organisation;
    using EA.Weee.RequestHandlers.AatfReturn;
    using EA.Weee.RequestHandlers.Admin.GetAatfs;
    using EA.Weee.RequestHandlers.Charges;
    using FluentAssertions;
    using Xunit;
    using DatabaseWrapper = Weee.Tests.Core.Model.DatabaseWrapper;
    using ModelHelper = Weee.Tests.Core.Model.ModelHelper;

    public class GetAatfsDataAccessTests
    {
        [Fact]
        public async Task GetAatfsDataAccess_ReturnsAatfsList()
        {
            using (var database = new DatabaseWrapper())
            {
                var helper = new ModelHelper(database.Model);
                var dataAccess = new GetAatfsDataAccess(database.WeeeContext);
                var genericDataAccess = new GenericDataAccess(database.WeeeContext);
                var competentAuthorityDataAccess = new CommonDataAccess(database.WeeeContext);
                var competentAuthority = await competentAuthorityDataAccess.FetchCompetentAuthority(CompetentAuthority.England);
                var country = await database.WeeeContext.Countries.SingleAsync(c => c.Name == "UK - England");
                var aatfContact = new AatfContact("first", "last", "position", "address1", "address2", "town", "county", "postcode", country, "telephone", "email");
                var organisation = Organisation.CreatePartnership("Koalas");
                var aatfAddress = CreateAatfSiteAddress(database);
                var aatfSize = AatfSize.Large;

                var aatf = new Aatf("KoalaBears", competentAuthority, "123456789", AatfStatus.Approved, organisation, aatfAddress, aatfSize, DateTime.Now, aatfContact, FacilityType.Aatf);

                await genericDataAccess.Add<Aatf>(aatf);

                var aatfList = await dataAccess.GetAatfs();
                aatfList.Should().Contain(aatf);
            }
        }

        private AatfAddress CreateAatfSiteAddress(DatabaseWrapper database)
        {
            var country = database.WeeeContext.Countries.First();

            return new AatfAddress("Name", "Building", "Road", "Bath", "BANES", "BA2 2YU", country);
        }
    }
}
