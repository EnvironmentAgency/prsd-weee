﻿namespace EA.Weee.DataAccess.Mappings
{
    using Domain.Organisation;
    using System.Data.Entity.ModelConfiguration;

    internal class OrganisationStatusMapping : ComplexTypeConfiguration<OrganisationStatus>
    {
        public OrganisationStatusMapping()
        {
            Ignore(x => x.DisplayName);
            Property(x => x.Value).HasColumnName("OrganisationStatus");
        }
    }
}
