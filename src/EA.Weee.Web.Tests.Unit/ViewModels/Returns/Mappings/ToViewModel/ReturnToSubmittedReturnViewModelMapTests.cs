﻿namespace EA.Weee.Web.Tests.Unit.ViewModels.Returns.Mappings.ToViewModel
{
    using Core.AatfReturn;
    using Core.DataReturns;
    using Core.Organisations;
    using FluentAssertions;
    using System;
    using Web.ViewModels.Returns.Mappings.ToViewModel;
    using Weee.Tests.Core;
    using Xunit;

    public class ReturnToSubmittedReturnViewModelMapTests
    {
        private readonly ReturnToSubmittedReturnViewModelMap map;

        public ReturnToSubmittedReturnViewModelMapTests()
        {
            map = new ReturnToSubmittedReturnViewModelMap();
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
            var id = Guid.NewGuid();

            var quarterWindow = QuarterWindowTestHelper.GetDefaultQuarterWindow();
            var returnData = new ReturnData() { Id = id, Quarter = new Quarter(2019, QuarterType.Q1), QuarterWindow = quarterWindow, OrganisationData = new OrganisationData() { Id = Guid.NewGuid(), Name = "operator" } };

            var result = map.Map(returnData);

            result.Quarter.Should().Be("Q1");
            result.Year.Should().Be("2019");
            result.Period.Should().Be("Q1 Jan - Mar 2019");
            result.OrganisationId.Should().Be(returnData.OrganisationData.Id);
        }
    }
}
