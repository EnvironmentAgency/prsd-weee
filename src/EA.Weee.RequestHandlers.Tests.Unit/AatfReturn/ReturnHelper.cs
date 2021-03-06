﻿namespace EA.Weee.RequestHandlers.Tests.Unit.AatfReturn
{
    using Domain.AatfReturn;
    using Domain.DataReturns;
    using Domain.Organisation;
    using Domain.Scheme;
    using FakeItEasy;
    using System;

    public static class ReturnHelper
    {
        public static Return GetReturn()
        {
            var organisation = new Organisation();
            var scheme = A.Fake<Scheme>();

            return new Return(organisation, new Quarter(2019, QuarterType.Q1), Guid.NewGuid().ToString(), FacilityType.Aatf);
        }
    }
}
