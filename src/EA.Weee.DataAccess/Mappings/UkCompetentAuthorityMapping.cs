﻿namespace EA.Weee.DataAccess.Mappings
{
    using System.Data.Entity.ModelConfiguration;
    using Domain;
    internal class UKCompetentAuthorityMapping : EntityTypeConfiguration<UKCompetentAuthority>
    {
        public UKCompetentAuthorityMapping()
        {
            this.ToTable("CompetentAuthority", "Lookup");
        }
    }

    //internal class UKCompetentAuthorityMapping : ComplexTypeConfiguration<UKCompetentAuthority>
    //{
    //    public UKCompetentAuthorityMapping()
    //    {
    //        Ignore(x => x.DisplayName);
    //        Ignore(x => x.ShortName);
    //        Property(x => x.Value)
    //            .HasColumnName("CompetentAuthority");
    //    }
    //}
}