﻿namespace EA.Weee.Core.AatfReturn
{
    using System;
    using Prsd.Core.Domain;

    [Serializable]
    public class AatfSize : Enumeration
    {
        public static readonly AatfSize Small = new AatfSize(1, "Small");
        public static readonly AatfSize Large = new AatfSize(2, "Large");
        protected AatfSize()
        {
        }

        public AatfSize(int value, string displayName)
            : base(value, displayName)
        {
        }
    }
}
