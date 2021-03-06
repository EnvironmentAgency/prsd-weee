﻿namespace EA.Weee.RequestHandlers.Admin.AatfReports
{
    using Core.Admin;
    using Core.Shared;
    using DataAccess;
    using DataAccess.StoredProcedure;
    using EA.Prsd.Core;
    using Prsd.Core.Mediator;
    using Requests.Admin.AatfReports;
    using Security;
    using System;
    using System.Threading.Tasks;

    internal class GetUkNonObligatedWeeeReceivedDataCsvHandler : IRequestHandler<GetUkNonObligatedWeeeReceivedDataCsv, CSVFileData>
    {
        private readonly IWeeeAuthorization authorization;
        private readonly WeeeContext context;
        private readonly CsvWriterFactory csvWriterFactory;

        public GetUkNonObligatedWeeeReceivedDataCsvHandler(IWeeeAuthorization authorization, WeeeContext context,
            CsvWriterFactory csvWriterFactory)
        {
            this.authorization = authorization;
            this.context = context;
            this.csvWriterFactory = csvWriterFactory;
        }

        public async Task<CSVFileData> HandleAsync(GetUkNonObligatedWeeeReceivedDataCsv request)
        {
            authorization.EnsureCanAccessInternalArea();
            if (request.ComplianceYear == 0)
            {
                var message = $"Compliance year cannot be \"{request.ComplianceYear}\".";
                throw new ArgumentException(message);
            }

            var items = await context.StoredProcedures.GetUkNonObligatedWeeeReceivedByComplianceYear(request.ComplianceYear);

            var csvWriter = csvWriterFactory.Create<UkNonObligatedWeeeReceivedData>();
            csvWriter.DefineColumn(ReportConstants.QuarterColumnHeading, i => i.Quarter);
            csvWriter.DefineColumn(ReportConstants.CategoryColumnHeading, i => i.Category);
            csvWriter.DefineColumn(ReportConstants.NonObligatedColumnHeading, i => i.TotalNonObligatedWeeeReceived);
            csvWriter.DefineColumn(ReportConstants.NonObligatedDcfColumnHeading, i => i.TotalNonObligatedWeeeReceivedFromDcf);
            var fileContent = csvWriter.Write(items);

            var fileName = $"{request.ComplianceYear}_UK non-obligated WEEE received at AATFs_{SystemTime.UtcNow:ddMMyyyy_HHmm}.csv";

            return new CSVFileData
            {
                FileContent = fileContent,
                FileName = fileName
            };
        }
    }
}
