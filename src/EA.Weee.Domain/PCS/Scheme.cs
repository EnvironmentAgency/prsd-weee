﻿namespace EA.Weee.Domain.PCS
{
    using Prsd.Core.Domain;
    using System;
    using Organisation;

    public class Scheme : Entity
    {
        public virtual Organisation Organisation { get; private set; }

        public string ApprovalNumber { get; private set; }
    }
}
