﻿namespace EA.Weee.Core.AatfReturn
{
    using DataReturns;
    using System;
    using System.Collections.Generic;

    public class ReturnData
    {
        public Guid Id { get; set; }

        public Quarter Quarter { get; set; }

        public QuarterWindow QuarterWindow { get; set; }

        public List<NonObligatedData> NonObligatedData { get; set; }

        public List<ObligatedData> ObligatedData { get; set; }

        public OperatorData ReturnOperatorData { get; set; }
    }
}
