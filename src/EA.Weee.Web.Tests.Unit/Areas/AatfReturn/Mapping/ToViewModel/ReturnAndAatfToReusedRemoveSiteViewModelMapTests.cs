﻿namespace EA.Weee.Web.Tests.Unit.Areas.AatfReturn.Mapping.ToViewModel
{
    using EA.Weee.Core.AatfReturn;
    using EA.Weee.Web.Areas.AatfReturn.Mappings.ToViewModel;
    using FluentAssertions;
    using System;
    using Xunit;

    public class ReturnAndAatfToReusedRemoveSiteViewModelMapTests
    {
        private readonly ReturnAndAatfToReusedRemoveSiteViewModelMap mapper;

        public ReturnAndAatfToReusedRemoveSiteViewModelMapTests()
        {
            mapper = new ReturnAndAatfToReusedRemoveSiteViewModelMap();
        }

        [Fact]
        public void Map_GivenNullSource_ArgumentNullExceptionExpected()
        {
            Action action = () => mapper.Map(null);

            action.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void Map_GivenValidSource_PropertiesShouldBeMapped()
        {
            var orgId = Guid.NewGuid();
            var aatfId = Guid.NewGuid();
            var returnId = Guid.NewGuid();
            var siteId = Guid.NewGuid();
            var siteAddress = "SITE ADDRESS";
            var site = new SiteAddressData("TEST NAME", "TEST", "TEST", "TEST", "TEST", "TEST", Guid.NewGuid(), "TEST");

            var transfer = new ReturnAndAatfToReusedRemoveSiteViewModelMapTransfer()
            {
                ReturnId = returnId,
                AatfId = aatfId,
                OrganisationId = orgId,
                SiteAddress = siteAddress,
                SiteId = siteId,
                Site = site
            };

            var result = mapper.Map(transfer);

            result.OrganisationId.Should().Be(orgId);
            result.ReturnId.Should().Be(returnId);
            result.AatfId.Should().Be(aatfId);
            result.SiteAddress.Should().Be(siteAddress);
            result.Site.Should().Be(site);
            result.SiteId.Should().Be(siteId);
            result.SiteAddressName.Should().Be(site.Name);
        }
    }
}
