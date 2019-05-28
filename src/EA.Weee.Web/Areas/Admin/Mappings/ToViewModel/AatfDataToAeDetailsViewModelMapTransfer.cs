﻿namespace EA.Weee.Web.Areas.Admin.Mappings.ToViewModel
{
    using EA.Weee.Core.AatfReturn;
    using System.Collections.Generic;
    public class AatfDataToAeDetailsViewModelMapTransfer
    {
        public AatfDataToAeDetailsViewModelMapTransfer(AatfData aatfData)
        {
            this.AatfData = aatfData;
        }
        public AatfData AatfData { get; set; }

        public string OrganisationString { get; set; }

        public List<AatfDataList> AssociatedAatfs { get; set; }

        public List<Core.Scheme.SchemeData> AssociatedSchemes { get; set; }
    }
}