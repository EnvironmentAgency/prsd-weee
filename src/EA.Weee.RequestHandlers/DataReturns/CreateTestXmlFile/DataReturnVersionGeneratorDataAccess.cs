﻿namespace EA.Weee.RequestHandlers.DataReturns.CreateTestXmlFile
{
    using Domain.Producer;
    using EA.Weee.DataAccess;
    using EA.Weee.Domain.Scheme;
    using System;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Linq;
    using System.Threading.Tasks;

    public class DataReturnVersionGeneratorDataAccess : IDataReturnVersionGeneratorDataAccess
    {
        private readonly WeeeContext context;

        public DataReturnVersionGeneratorDataAccess(WeeeContext context)
        {
            this.context = context;
        }

        public async Task<Domain.Scheme.Scheme> FetchSchemeAsync(Guid organisationID)
        {
            return await context
                .Schemes
                .SingleAsync(s => s.OrganisationId == organisationID);
        }

        public async Task<IList<RegisteredProducer>> FetchRegisteredProducersAsync(Scheme scheme, int year)
        {
            return await context
                .RegisteredProducers
                .Where(rp => rp.Scheme.Id == scheme.Id)
                .Where(rp => rp.ComplianceYear == year)
                .Where(rp => rp.CurrentSubmission != null)
                .Include(rp => rp.CurrentSubmission)
                .ToListAsync();
        }
    }
}
