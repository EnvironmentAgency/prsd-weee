﻿namespace EA.Weee.DataAccess.Mappings
{
    using EA.Weee.Domain.DataReturns;
    using System.Data.Entity.ModelConfiguration;

    internal class WeeeCollectedAmountMapping : EntityTypeConfiguration<WeeeCollectedAmount>
    {
        public WeeeCollectedAmountMapping()
        {
            ToTable("WeeeCollectedAmount", "PCS");

            Ignore(ps => ps.ObligationType);
            Property(ps => ps.DatabaseObligationType).HasColumnName("ObligationType");

            Property(ps => ps.Tonnage).HasPrecision(28, 3);
        }
    }
}
