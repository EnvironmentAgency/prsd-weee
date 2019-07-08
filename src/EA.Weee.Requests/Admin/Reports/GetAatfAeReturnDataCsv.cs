﻿namespace EA.Weee.Requests.Admin.Reports
{
    using System;
    using Core.Admin;
    using Core.Shared;
    using EA.Weee.Core.AatfReturn;
    using Prsd.Core.Mediator;

    public class GetAatfAeReturnDataCsv : IRequest<CSVFileData>
    {
        public int ComplianceYear { get; private set; }
        public int Quarter { get; private set; }
        public FacilityType FacilityType { get; private set; }

        public int? ReturnStatus { get; private set; }
        public Guid? AuthorityId { get; private set; }

        public Guid? PanArea { get; private set; }

        public Guid? Area { get; private set; }

        public GetAatfAeReturnDataCsv(int complianceYear,
          int quarter, FacilityType facilityType, int? returnStatus, Guid? authority, Guid? panArea, Guid? area)
        {
            ComplianceYear = complianceYear;
            Quarter = quarter;
            FacilityType = facilityType;
            ReturnStatus = returnStatus;
            AuthorityId = authority;
            PanArea = panArea;
            Area = area;
        }
    }
}
