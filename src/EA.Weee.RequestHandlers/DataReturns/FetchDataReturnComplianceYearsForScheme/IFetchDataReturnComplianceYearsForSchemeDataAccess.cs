﻿namespace EA.Weee.RequestHandlers.DataReturns.FetchDataReturnComplianceYearsForScheme
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    public interface IFetchDataReturnComplianceYearsForSchemeDataAccess
    {
        Task<List<int>> GetDataReturnComplianceYearsForScheme(Guid schemeId);
        Task<Domain.Scheme.Scheme> FetchSchemeByOrganisationIdAsync(Guid organisationId);
    }
}
