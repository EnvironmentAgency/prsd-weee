﻿namespace EA.Weee.Web.Areas.AatfReturn.Mappings.ToViewModel
{
    using System;
    using System.Collections.Generic;
    using EA.Weee.Core.AatfReturn;

    public class ReportOptionsToSelectReportOptionsViewModelMapTransfer
    {
        public Guid OrganisationId { get; set; }

        public Guid ReturnId { get; set; }

        public List<ReportOnQuestion> ReportOnQuestions { get; set; }
    }
}