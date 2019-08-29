﻿namespace EA.Weee.Web.Tests.Unit.Areas.Aatf.Mapping.ToViewModel
{
    using EA.Weee.Core.AatfReturn;
    using EA.Weee.Web.Areas.Aatf.Mappings.ToViewModel;
    using FakeItEasy;
    using FluentAssertions;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Xunit;

    public class AatfDataToHomeViewModelMapTests
    {
        private readonly AatfDataToHomeViewModelMap map;

        public AatfDataToHomeViewModelMapTests()
        {
            this.map = new AatfDataToHomeViewModelMap();
        }

        [Fact]
        public void Map_GivenNullSource_ArgumentNullExceptionExpected()
        {
            Action action = () => map.Map(null);

            action.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void Map_GivenListOfAatfs_ContactDetailsNameIsBuilt()
        {
            var organisationId = Guid.NewGuid();
            var aatfList = new List<AatfData>();

            var aatfData = new AatfData(Guid.NewGuid(), "AATF", "approval number", 2019, A.Dummy<Core.Shared.UKCompetentAuthorityData>(),
                   Core.AatfReturn.AatfStatus.Approved, A.Dummy<AatfAddressData>(), Core.AatfReturn.AatfSize.Large, DateTime.Now,
                   A.Dummy<Core.Shared.PanAreaData>(), null)
            {
                FacilityType = FacilityType.Aatf
            };

            aatfList.Add(aatfData);

            var transfer = new AatfDataToHomeViewModelMapTransfer() { OrganisationId = organisationId, AatfList = aatfList, IsAE = false };

            var result = map.Map(transfer);

            foreach (var aatf in result.AatfList)
            {
                aatf.AatfContactDetailsName.Should().Be(aatf.Name + " (" + aatf.ApprovalNumber + ") - " + aatf.AatfStatus);
            }
        }

        [Fact]
        public void Map_GivenListOfAatfs_GivenAatfsWithTheSameAatfIdTheLatestApprovalDateShouldBeReturned()
        {
            var organisationId = Guid.NewGuid();
            var aatfList = new List<AatfData>();
            var aatfId = Guid.NewGuid();

            var aatfData = new AatfData(Guid.NewGuid(), "AATF", "approval number", 2019, A.Dummy<Core.Shared.UKCompetentAuthorityData>(),
                   Core.AatfReturn.AatfStatus.Approved, A.Dummy<AatfAddressData>(), Core.AatfReturn.AatfSize.Large, new DateTime(2018, 10, 1),
                   A.Dummy<Core.Shared.PanAreaData>(), null)
            {
                FacilityType = FacilityType.Aatf,
                AatfId = aatfId
            };

            var aatfData2 = new AatfData(Guid.NewGuid(), "AATF 2020", "approval number", 2020, A.Dummy<Core.Shared.UKCompetentAuthorityData>(),
               Core.AatfReturn.AatfStatus.Approved, A.Dummy<AatfAddressData>(), Core.AatfReturn.AatfSize.Large, new DateTime(2019, 10, 1),
               A.Dummy<Core.Shared.PanAreaData>(), null)
            {
                FacilityType = FacilityType.Aatf,
                AatfId = aatfId
            };

            aatfList.Add(aatfData);
            aatfList.Add(aatfData2);

            var transfer = new AatfDataToHomeViewModelMapTransfer() { OrganisationId = organisationId, AatfList = aatfList, IsAE = false };

            var result = map.Map(transfer);

            result.AatfList.Count.Should().Be(1);
            result.AatfList.Should().Contain(aatfData2);
            result.AatfList.Should().NotContain(aatfData);
        }

        [Fact]
        public void Map_GivenListOfAatfs_ListIdOrderedByName()
        {
            var organisationId = Guid.NewGuid();
            var aatfList = new List<AatfData>();

            var aatfData = new AatfData(Guid.NewGuid(), "Banana", "approval number", 2019, A.Dummy<Core.Shared.UKCompetentAuthorityData>(),
                   Core.AatfReturn.AatfStatus.Approved, A.Dummy<AatfAddressData>(), Core.AatfReturn.AatfSize.Large, DateTime.Now,
                   A.Dummy<Core.Shared.PanAreaData>(), null)
            {
                FacilityType = FacilityType.Aatf
            };

            var aatfData2 = new AatfData(Guid.NewGuid(), "Carrot", "approval number", 2019, A.Dummy<Core.Shared.UKCompetentAuthorityData>(),
               Core.AatfReturn.AatfStatus.Approved, A.Dummy<AatfAddressData>(), Core.AatfReturn.AatfSize.Large, DateTime.Now,
               A.Dummy<Core.Shared.PanAreaData>(), null)
            {
                FacilityType = FacilityType.Aatf
            };

            var aatfData3 = new AatfData(Guid.NewGuid(), "Apple", "approval number", 2019, A.Dummy<Core.Shared.UKCompetentAuthorityData>(),
               Core.AatfReturn.AatfStatus.Approved, A.Dummy<AatfAddressData>(), Core.AatfReturn.AatfSize.Large, DateTime.Now,
               A.Dummy<Core.Shared.PanAreaData>(), null)
            {
                FacilityType = FacilityType.Aatf
            };

            aatfList.Add(aatfData);
            aatfList.Add(aatfData2);
            aatfList.Add(aatfData3);

            var transfer = new AatfDataToHomeViewModelMapTransfer() { OrganisationId = organisationId, AatfList = aatfList, IsAE = false };

            var result = map.Map(transfer);

            result.AatfList.Should().BeInAscendingOrder(z => z.Name);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void Map_GivenIsAE_AatfListShouldOnlyContainThatIsFacilityType(bool isAE)
        {
            var organisationId = Guid.NewGuid();
            var aatfList = new List<AatfData>();

            var aatfData = new AatfData(Guid.NewGuid(), "AATF", "approval number", 2019, A.Dummy<Core.Shared.UKCompetentAuthorityData>(),
                   Core.AatfReturn.AatfStatus.Approved, A.Dummy<AatfAddressData>(), Core.AatfReturn.AatfSize.Large, DateTime.Now,
                   A.Dummy<Core.Shared.PanAreaData>(), null)
            {
                FacilityType = FacilityType.Aatf,
                AatfId = Guid.NewGuid()
            };

            var aatfData2 = new AatfData(Guid.NewGuid(), "AATF2", "approval number", 2019, A.Dummy<Core.Shared.UKCompetentAuthorityData>(),
                   Core.AatfReturn.AatfStatus.Approved, A.Dummy<AatfAddressData>(), Core.AatfReturn.AatfSize.Large, DateTime.Now,
                   A.Dummy<Core.Shared.PanAreaData>(), null)
            {
                FacilityType = FacilityType.Aatf,
                AatfId = Guid.NewGuid()
            };

            var exporterData = new AatfData(Guid.NewGuid(), "AE", "approval number", 2019, A.Dummy<Core.Shared.UKCompetentAuthorityData>(),
                   Core.AatfReturn.AatfStatus.Approved, A.Dummy<AatfAddressData>(), Core.AatfReturn.AatfSize.Large, DateTime.Now,
                   A.Dummy<Core.Shared.PanAreaData>(), null)
            {
                FacilityType = FacilityType.Ae,
                AatfId = Guid.NewGuid()
            };

            var exporterData2 = new AatfData(Guid.NewGuid(), "AE", "approval number", 2019, A.Dummy<Core.Shared.UKCompetentAuthorityData>(),
               Core.AatfReturn.AatfStatus.Approved, A.Dummy<AatfAddressData>(), Core.AatfReturn.AatfSize.Large, DateTime.Now,
               A.Dummy<Core.Shared.PanAreaData>(), null)
            {
                FacilityType = FacilityType.Ae,
                AatfId = Guid.NewGuid()
            };

            aatfList.Add(aatfData);
            aatfList.Add(exporterData);
            aatfList.Add(aatfData2);
            aatfList.Add(exporterData2);

            var transfer = new AatfDataToHomeViewModelMapTransfer() { OrganisationId = organisationId, AatfList = aatfList, IsAE = isAE };

            var result = map.Map(transfer);

            result.AatfList.Should().NotBeEmpty();

            if (isAE)
            {
                result.AatfList.Should().OnlyContain(m => m.FacilityType == FacilityType.Ae);
            }
            else
            {
                result.AatfList.Should().OnlyContain(m => m.FacilityType == FacilityType.Aatf);
            }
        }

        [Fact]
        public void Map_GivenSource_PropertiesShouldBeSet()
        {
            var transfer = new AatfDataToHomeViewModelMapTransfer() { OrganisationId = Guid.NewGuid(), AatfList = A.Fake<List<AatfData>>(), IsAE = false };

            var result = map.Map(transfer);

            result.AatfList.Should().BeEquivalentTo(transfer.AatfList);
            result.OrganisationId.Should().Be(transfer.OrganisationId);
            result.IsAE.Should().Be(transfer.IsAE);
        }
    }
}
