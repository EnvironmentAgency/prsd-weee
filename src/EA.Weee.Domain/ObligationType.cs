﻿namespace EA.Weee.Domain
{
    using EA.Prsd.Core.Domain;

    public class ObligationType : Enumeration
    {
        public static readonly ObligationType B2B = new ObligationType(1, "B2B");
        public static readonly ObligationType B2C = new ObligationType(2, "B2C");
        public static readonly ObligationType Both = new ObligationType(3, "Both");

        protected ObligationType()
        {
        }

        private ObligationType(int value, string displayName)
            : base(value, displayName)
        {
        }
    }
}
