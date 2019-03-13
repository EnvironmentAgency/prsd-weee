﻿namespace EA.Weee.Web.Tests.Unit.Areas.AatfReturn.Mapping.ToViewModel
{
    using EA.Weee.Web.Areas.AatfReturn.Mappings.ToViewModel;
    using EA.Weee.Web.Services.Caching;
    using FakeItEasy;
    using FluentAssertions;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Xunit;

    public class ReturnAndAatfToSentOnCreateSiteViewModelMapTests
    {
        private readonly ReturnAndAatfToSentOnCreateSiteViewModelMap map;

        public ReturnAndAatfToSentOnCreateSiteViewModelMapTests()
        {
            map = new ReturnAndAatfToSentOnCreateSiteViewModelMap(A.Fake<IWeeeCache>());
        }

        [Fact]
        public void Map_GivenNullSource_ArgumentNullExceptionExpected()
        {
            Action action = () => map.Map(null);

            action.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void Map_GivenValidSource_PropertiesShouldBeMapped()
        {
            var orgId = Guid.NewGuid();
            var aatfId = Guid.NewGuid();
            var returnId = Guid.NewGuid();
            var transfer = new ReturnAndAatfToSentOnCreateSiteViewModelMapTransfer(A.Fake<IList<Core.Shared.CountryData>>())
            {
                ReturnId = returnId,
                AatfId = aatfId,
                OrganisationId = orgId
            };

            var result = map.Map(transfer);

            result.OrganisationId.Should().Be(orgId);
            result.ReturnId.Should().Be(returnId);
            result.AatfId.Should().Be(aatfId);
        }
    }
}