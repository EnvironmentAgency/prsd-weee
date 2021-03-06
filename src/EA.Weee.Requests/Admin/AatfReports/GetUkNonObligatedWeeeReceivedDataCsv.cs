﻿namespace EA.Weee.Requests.Admin.AatfReports
{
    using Core.Admin;
    using Prsd.Core.Mediator;

    public class GetUkNonObligatedWeeeReceivedDataCsv : IRequest<CSVFileData>
    {
        public int ComplianceYear { get; private set; }

        public GetUkNonObligatedWeeeReceivedDataCsv(int complianceYear)
        {
            ComplianceYear = complianceYear;
        }
    }
}
