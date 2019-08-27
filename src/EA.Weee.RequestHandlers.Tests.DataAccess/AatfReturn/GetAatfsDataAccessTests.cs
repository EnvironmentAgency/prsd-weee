﻿namespace EA.Weee.RequestHandlers.Tests.DataAccess.AatfReturn
{
    using EA.Weee.Core.Shared;
    using EA.Weee.Domain.AatfReturn;
    using EA.Weee.Domain.Organisation;
    using EA.Weee.RequestHandlers.AatfReturn;
    using EA.Weee.RequestHandlers.Admin.GetAatfs;
    using FluentAssertions;
    using RequestHandlers.Shared;
    using System;
    using System.Data.Entity;
    using System.Linq;
    using System.Threading.Tasks;
    using Weee.Tests.Core;
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
                var aatfAddress = AddressHelper.GetAatfAddress(database);
                var aatfSize = AatfSize.Large;

                var aatf = ObligatedWeeeIntegrationCommon.CreateAatf(database, organisation);

                await genericDataAccess.Add<Aatf>(aatf);

                var aatfList = await dataAccess.GetAatfs();
                aatfList.Should().Contain(aatf);
            }
        }

        [Fact]
        public async Task GetFilteredAatfsDataAccess_ByApprovalNumber_ReturnsFilteredAatfsList()
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
                var aatfAddress = AddressHelper.GetAatfAddress(database);
                var aatfSize = AatfSize.Large;

                var aatf = new Aatf("KoalaBears", competentAuthority, "WEE/AB1289YZ/ATF", AatfStatus.Approved, organisation, aatfAddress, aatfSize, DateTime.Now, aatfContact, FacilityType.Aatf, 2019, database.WeeeContext.LocalAreas.First(), database.WeeeContext.PanAreas.First());

                await genericDataAccess.Add<Aatf>(aatf);

                var filteredListWithAatf = await dataAccess.GetFilteredAatfs(new Core.AatfReturn.AatfFilter { ApprovalNumber = "a" });
                var filteredListWithoutAatf = await dataAccess.GetFilteredAatfs(new Core.AatfReturn.AatfFilter { ApprovalNumber = "q" });

                filteredListWithAatf.Should().Contain(aatf);
                filteredListWithoutAatf.Should().NotContain(aatf);
            }
        }

        [Fact]
        public async Task GetFilteredAatfsDataAccess_ByName_ReturnsFilteredAatfsList()
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
                var aatfAddress = AddressHelper.GetAatfAddress(database);
                var aatfSize = AatfSize.Large;

                var aatf = new Aatf("KoalaBears", competentAuthority, "WEE/AB1289YZ/ATF", AatfStatus.Approved, organisation, aatfAddress, aatfSize, DateTime.Now, aatfContact, FacilityType.Aatf, 2019, database.WeeeContext.LocalAreas.First(), database.WeeeContext.PanAreas.First());

                await genericDataAccess.Add<Aatf>(aatf);

                var filteredListWithAatf = await dataAccess.GetFilteredAatfs(new Core.AatfReturn.AatfFilter { Name = "k" });
                var filteredListWithoutAatf = await dataAccess.GetFilteredAatfs(new Core.AatfReturn.AatfFilter { Name = "z" });

                filteredListWithAatf.Should().Contain(aatf);
                filteredListWithoutAatf.Should().NotContain(aatf);
            }
        }
    }
}
