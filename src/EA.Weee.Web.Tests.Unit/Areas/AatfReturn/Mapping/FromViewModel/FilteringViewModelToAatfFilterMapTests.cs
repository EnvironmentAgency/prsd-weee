﻿namespace EA.Weee.Web.Tests.Unit.Areas.AatfReturn.Mapping.ToViewModel
{
    using AutoFixture;
    using EA.Weee.Core.AatfReturn;
    using EA.Weee.Web.Areas.Admin.Mappings.ToViewModel;
    using EA.Weee.Web.Areas.Admin.ViewModels.Aatf;
    using FluentAssertions;
    using System;
    using Xunit;

    public class FilteringViewModelToAatfFilterMapTests
    {
        private readonly FilteringViewModelToAatfFilterMap map;
        private readonly Fixture fixture;

        public FilteringViewModelToAatfFilterMapTests()
        {
            map = new FilteringViewModelToAatfFilterMap();
            fixture = new Fixture();
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
            var viewModel = fixture.Build<FilteringViewModel>().With(a => a.FacilityType, FacilityType.Aatf).Create();

            var result = map.Map(viewModel);

            Assert.Equal(viewModel.ApprovalNumber, result.ApprovalNumber);
            Assert.Equal(viewModel.Name, result.Name);
            Assert.Equal(viewModel.SelectedAuthority, result.SelectedAuthority);
            Assert.Equal(viewModel.SelectedStatus, result.SelectedStatus);
        }
    }
}
