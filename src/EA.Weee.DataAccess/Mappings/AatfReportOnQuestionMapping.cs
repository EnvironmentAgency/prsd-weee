﻿namespace EA.Weee.DataAccess.Mappings
{
    using EA.Weee.Domain.AatfReturn;
    using System.Data.Entity.ModelConfiguration;

    internal class AatfReportOnQuestionMapping : EntityTypeConfiguration<ReportOnQuestion>
    {
        public AatfReportOnQuestionMapping()
        {
            ToTable("ReportOnQuestion", "AATF");
        }
    }
}
