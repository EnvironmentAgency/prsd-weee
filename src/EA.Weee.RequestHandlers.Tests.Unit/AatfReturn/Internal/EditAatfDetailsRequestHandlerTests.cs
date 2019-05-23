﻿namespace EA.Weee.RequestHandlers.Tests.Unit.AatfReturn.Internal
{
    using System;
    using System.Security;
    using System.Threading.Tasks;
    using AutoFixture;
    using EA.Prsd.Core.Mapper;
    using EA.Weee.Core.AatfReturn;
    using EA.Weee.Core.Shared;
    using EA.Weee.Domain.AatfReturn;
    using EA.Weee.RequestHandlers.AatfReturn;
    using EA.Weee.RequestHandlers.AatfReturn.Internal;
    using EA.Weee.RequestHandlers.Security;
    using EA.Weee.Requests.AatfReturn.Internal;
    using EA.Weee.Security;
    using EA.Weee.Tests.Core;
    using FakeItEasy;
    using FluentAssertions;
    using Xunit;

    public class EditAatfDetailsRequestHandlerTests
    {
        private readonly Fixture fixture;
        private readonly IWeeeAuthorization authorization;
        private readonly IGenericDataAccess genericDataAccess;
        private readonly IAatfDataAccess aatfDataAccess;
        private readonly IMap<AatfAddressData, AatfAddress> addressMapper;
        private readonly EditAatfDetailsRequestHandler handler;

        public EditAatfDetailsRequestHandlerTests()
        {
            fixture = new Fixture();
            authorization = A.Fake<IWeeeAuthorization>();
            genericDataAccess = A.Fake<IGenericDataAccess>();
            aatfDataAccess = A.Fake<IAatfDataAccess>();
            addressMapper = A.Fake<IMap<AatfAddressData, AatfAddress>>();

            handler = new EditAatfDetailsRequestHandler(authorization, aatfDataAccess, genericDataAccess, addressMapper);
        }

        [Fact]
        public async Task HandleAsync_NoInternalAccess_ThrowsSecurityException()
        {
            var authorization = new AuthorizationBuilder().DenyInternalAreaAccess().Build();

            var handler = new EditAatfDetailsRequestHandler(authorization, aatfDataAccess, genericDataAccess, addressMapper);

            Func<Task> action = async () => await handler.HandleAsync(A.Dummy<EditAatfDetails>());

            await action.Should().ThrowAsync<SecurityException>();
        }

        [Fact]
        public async Task HandleAsync_NoAdminRoleAccess_ThrowsSecurityException()
        {
            var authorization = new AuthorizationBuilder().AllowInternalAreaAccess().DenyRole(Roles.InternalAdmin).Build();

            var handler = new EditAatfDetailsRequestHandler(authorization, aatfDataAccess, genericDataAccess, addressMapper);

            Func<Task> action = async () => await handler.HandleAsync(A.Dummy<EditAatfDetails>());

            await action.Should().ThrowAsync<SecurityException>();
        }

        [Fact]
        public async Task HandleAsync_GivenMessageContainingUpdatedAddress_DetailsAreUpdatedCorrectly()
        {
            var competentAuthority = fixture.Create<UKCompetentAuthorityData>();
            var data = fixture.Build<AatfData>()
                .With(e => e.CompetentAuthority, competentAuthority)
                .With(e => e.AatfStatus, Core.AatfReturn.AatfStatus.Approved)
                .With(e => e.Size, Core.AatfReturn.AatfSize.Large)
                .Create();
            var updateRequest = fixture.Build<EditAatfDetails>().With(e => e.Data, data).Create();
            var siteAddress = fixture.Create<AatfAddress>();
            A.CallTo(() => addressMapper.Map(data.SiteAddress)).Returns(siteAddress);

            var aatf = A.Fake<Aatf>();

            A.CallTo(() => genericDataAccess.GetById<Aatf>(updateRequest.Data.Id)).Returns(aatf);

            var result = await handler.HandleAsync(updateRequest);

            Assert.True(result);
            A.CallTo(() => aatfDataAccess.UpdateDetails(aatf, A<Aatf>.That.Matches(a => a.Name == data.Name &&
                a.CompetentAuthorityId == competentAuthority.Id &&
                a.ApprovalNumber == data.ApprovalNumber &&
                a.AatfStatus == Domain.AatfReturn.AatfStatus.Approved &&
                a.SiteAddress == siteAddress &&
                a.Size == Domain.AatfReturn.AatfSize.Large &&
                a.ApprovalDate == data.ApprovalDate.GetValueOrDefault()))).MustHaveHappenedOnceExactly();
        }
    }
}