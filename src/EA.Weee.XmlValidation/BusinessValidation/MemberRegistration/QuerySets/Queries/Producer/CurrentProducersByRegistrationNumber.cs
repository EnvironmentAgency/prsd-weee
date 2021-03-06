﻿namespace EA.Weee.XmlValidation.BusinessValidation.MemberRegistration.QuerySets.Queries.Producer
{
    using DataAccess;
    using Domain.Producer;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Linq;

    public class CurrentProducersByRegistrationNumber : Query<Dictionary<string, List<ProducerSubmission>>>, ICurrentProducersByRegistrationNumber
    {
        public CurrentProducersByRegistrationNumber(WeeeContext context)
        {
            query = () => context
                .RegisteredProducers
                .Where(rp => rp.CurrentSubmission != null)
                .Select(rp => rp.CurrentSubmission)
                .Include(p => p.MemberUpload)
                .Include(p => p.RegisteredProducer.Scheme)
                .Include(p => p.ProducerBusiness)
                .Include(p => p.ProducerBusiness.CompanyDetails)
                .Include(p => p.ProducerBusiness.Partnership)
                .AsNoTracking()
                .GroupBy(p => p.RegisteredProducer.ProducerRegistrationNumber)
                .ToDictionary(g => g.Key, p => p.ToList());
        }
    }
}
