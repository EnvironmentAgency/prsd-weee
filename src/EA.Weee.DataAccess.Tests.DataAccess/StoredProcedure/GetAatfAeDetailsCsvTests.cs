﻿namespace EA.Weee.DataAccess.Tests.DataAccess.StoredProcedure
{
    using EA.Weee.DataAccess.StoredProcedure;
    using EA.Weee.Domain.AatfReturn;
    using EA.Weee.Tests.Core;
    using EA.Weee.Tests.Core.Model;
    using FluentAssertions;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using AutoFixture;
    using Domain;
    using Xunit;
    using Aatf = Domain.AatfReturn.Aatf;
    using Address = Domain.Organisation.Address;
    using Organisation = Domain.Organisation.Organisation;

    public class GetAatfAeDetailsCsvTests
    {
        private readonly Fixture fixture;

        public GetAatfAeDetailsCsvTests()
        {
            fixture = new Fixture();
        }

        [Theory]
        [InlineData(false, false)]
        [InlineData(true, false)]
        [InlineData(false, true)]
        [InlineData(true, true)]
        public async Task Execute_GivenComplianceAndFacilityTypeAndLocalAreaAndPanArea_ReturnsData(bool hasArea, bool hasPan)
        {
            using (var db = new DatabaseWrapper())
            {
                var org = OrgWithAddress(db);
                var aatf = ObligatedWeeeIntegrationCommon.CreateAatf(db, org, 2019, hasArea, hasPan, fixture.Create<string>());
                var aatfs = new List<Aatf>() { aatf };

                db.WeeeContext.Organisations.Add(org);
                db.WeeeContext.Aatfs.Add(aatf);
                await db.WeeeContext.SaveChangesAsync();

                Guid? areaId = null;
                if (hasArea)
                {
                    areaId = aatf.LocalAreaId;
                }

                Guid? panId = null;
                if (hasPan)
                {
                    panId = aatf.PanAreaId;
                }

                var results = await db.StoredProcedures.GetAatfAeDetailsCsvData(2019, 1, aatf.CompetentAuthority.Id, areaId, panId);
                
                VerifyResult(results, aatfs);              
            }
        }

        [Fact]
        public async Task Execute_GivenComplianceAndFacilityTypeAndEA_ReturnsData()
        {
            using (var db = new DatabaseWrapper())
            {
                var org = OrgWithAddress(db);
                var aatf = ObligatedWeeeIntegrationCommon.CreateAatf(db, org, 2019, false, false, fixture.Create<string>());
                var aatfs = new List<Aatf>() { aatf };

                db.WeeeContext.Organisations.Add(org);
                db.WeeeContext.Aatfs.Add(aatf);
                await db.WeeeContext.SaveChangesAsync();

                var results = await db.StoredProcedures.GetAatfAeDetailsCsvData(2019, 1, aatf.CompetentAuthority.Id, null, null);

                VerifyResult(results, aatfs);
            }
        }

        [Fact]
        public async Task Execute_GivenComplianceAndFacilityTypeOfAe_ReturnsData()
        {
            using (var db = new DatabaseWrapper())
            {
                var org = OrgWithAddress(db);
                var ae = ObligatedWeeeIntegrationCommon.CreateAe(db, org);
                var aatfs = new List<Aatf>() { ae };

                db.WeeeContext.Aatfs.Add(ae);
                await db.WeeeContext.SaveChangesAsync();

                var results = await db.StoredProcedures.GetAatfAeDetailsCsvData(2019, 2, ae.CompetentAuthority.Id, null, null);

                VerifyResult(results, aatfs);
            }
        }

        private void VerifyResult(List<AatfAeDetailsData> results, List<Aatf> aatfs)
        {
            foreach (var verifyAatf in aatfs)
            {
                results.Count(x => x.Name == verifyAatf.Name).Should().Be(1);
                var aatf = results.First(x => x.Name == verifyAatf.Name);
                aatf.ComplianceYear.Should().Be(verifyAatf.ComplianceYear);
                aatf.AppropriateAuthorityAbbr.Should().Be(verifyAatf.CompetentAuthority.Abbreviation);
                aatf.EaArea.Should().Be(verifyAatf.LocalArea?.Name);
                aatf.PanAreaTeam.Should().Be(verifyAatf.PanArea?.Name);
                aatf.Name.Should().Be(verifyAatf.Name);
                aatf.Address1.Should().Be(verifyAatf.SiteAddress.Address1);
                aatf.Address2.Should().Be(verifyAatf.SiteAddress.Address2);
                aatf.TownCity.Should().Be(verifyAatf.SiteAddress.TownOrCity);
                aatf.CountyRegion.Should().Be(verifyAatf.SiteAddress.CountyOrRegion);
                aatf.Country.Should().Be(verifyAatf.SiteAddress.Country.Name);
                aatf.PostCode.Should().Be(verifyAatf.SiteAddress.Postcode);
                aatf.ApprovalNumber.Should().Be(verifyAatf.ApprovalNumber);
                aatf.ApprovalDate.Should().Be(verifyAatf.ApprovalDate.GetValueOrDefault());
                aatf.Size.Should().Be(verifyAatf.Size.DisplayName);
                aatf.Status.Should().Be(verifyAatf.AatfStatus.DisplayName);
                aatf.FirstName.Should().Be(verifyAatf.Contact.FirstName);
                aatf.LastName.Should().Be(verifyAatf.Contact.LastName);
                aatf.ContactPosition.Should().Be(verifyAatf.Contact.Position);
                aatf.ContactAddress1.Should().Be(verifyAatf.Contact.Address1);
                aatf.ContactAddress2.Should().Be(verifyAatf.Contact.Address2);
                aatf.ContactTownCity.Should().Be(verifyAatf.Contact.TownOrCity);
                aatf.ContactCountyRegion.Should().Be(verifyAatf.Contact.CountyOrRegion);
                aatf.ContactCountry.Should().Be(verifyAatf.Contact.Country.Name);
                aatf.ContactPostcode.Should().Be(verifyAatf.Contact.Postcode);
                aatf.ContactEmail.Should().Be(verifyAatf.Contact.Email);
                aatf.ContactPhone.Should().Be(verifyAatf.Contact.Telephone);
                aatf.OrganisationName.Should().Be(verifyAatf.Organisation.OrganisationName);
                aatf.OrganisationAddress1.Should().Be(verifyAatf.Organisation.BusinessAddress.Address1);
                aatf.OrganisationAddress2.Should().Be(verifyAatf.Organisation.BusinessAddress.Address2);
                aatf.OrganisationTownCity.Should().Be(verifyAatf.Organisation.BusinessAddress.TownOrCity);
                aatf.OrganisationCountyRegion.Should().Be(verifyAatf.Organisation.BusinessAddress.CountyOrRegion);
                aatf.OrganisationCountry.Should().Be(verifyAatf.Organisation.BusinessAddress.Country.Name);
                aatf.OrganisationPostcode.Should().Be(verifyAatf.Organisation.BusinessAddress.Postcode);
                aatf.OperatorName.Should().Be(verifyAatf.Organisation.Name);
                aatf.OperatorTradingName.Should().Be(verifyAatf.Organisation.TradingName);
                aatf.AatfAddress.Should().Be(GetSiteAddressString(verifyAatf.SiteAddress));
                aatf.OperatorAddress.Should().Be(GetOrganisationAddressString(verifyAatf.Organisation.BusinessAddress));
            }
        }

        private string GetSiteAddressString(AatfAddress address)
        {
            var addressString = address.Address1 + ", ";

            if (address.Address2 != null)
            {
                addressString += address.Address2 + ", ";
            }

            addressString += address.TownOrCity + ", ";

            if (address.CountyOrRegion != null)
            {
                addressString += address.CountyOrRegion;
            }

            return addressString;
        }

        private string GetOrganisationAddressString(Domain.Organisation.Address address)
        {
            var addressString = address.Address1 + ", ";

            if (address.Address2 != null)
            {
                addressString += address.Address2 + ", ";
            }

            addressString += address.TownOrCity + ", ";

            if (address.CountyOrRegion != null)
            {
                addressString += address.CountyOrRegion;
            }

            return addressString;
        }

        private static Organisation OrgWithAddress(DatabaseWrapper db)
        {
            var org = Organisation.CreatePartnership("trading");
            var address = new Address("1", "street", "Woking", "Hampshire", "GU21 5EE", db.WeeeContext.Countries.First(), "12345678", "test@co.uk");
            org.AddOrUpdateAddress(AddressType.RegisteredOrPPBAddress, address);
            return org;
        }
    }
}
