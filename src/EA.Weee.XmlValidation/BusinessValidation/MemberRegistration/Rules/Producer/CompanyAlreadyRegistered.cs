﻿namespace EA.Weee.XmlValidation.BusinessValidation.MemberRegistration.Rules.Producer
{
    using Core.Helpers;
    using QuerySets;
    using System.Linq;
    using Xml.MemberRegistration;

    public class CompanyAlreadyRegistered : ICompanyAlreadyRegistered
    {
        private readonly IProducerQuerySet producerQuerySet;
        private readonly IMigratedProducerQuerySet migratedProducerQuerySet;

        public CompanyAlreadyRegistered(IProducerQuerySet producerQuerySet, IMigratedProducerQuerySet migratedProducerQuerySet)
        {
            this.producerQuerySet = producerQuerySet;
            this.migratedProducerQuerySet = migratedProducerQuerySet;
        }

        public RuleResult Evaluate(producerType element)
        {
            var result = RuleResult.Pass();

            if (element.status == statusType.I &&
                element.producerBusiness != null)
            {
                var company = element.producerBusiness.Item as companyType;
                if (company != null)
                {
                    var companyNumber = CompanyRegistrationNumberFormatter
                        .FormatCompanyRegistrationNumber(company.companyNumber);

                    if (!string.IsNullOrEmpty(companyNumber) &&
                        (producerQuerySet.GetLatestCompanyProducers().Any(p =>
                            {
                                var existingCompanyRegistrationNumber = CompanyRegistrationNumberFormatter
                                .FormatCompanyRegistrationNumber(p.ProducerBusiness.CompanyDetails.CompanyNumber);

                                return !string.IsNullOrEmpty(existingCompanyRegistrationNumber) &&
                                    existingCompanyRegistrationNumber == companyNumber;
                            })
                         ||
                         migratedProducerQuerySet.GetAllMigratedProducers().Any(m =>
                             {
                                 var migratedProducerCompanyNumber = CompanyRegistrationNumberFormatter
                                 .FormatCompanyRegistrationNumber(m.CompanyNumber);

                                 return !string.IsNullOrEmpty(migratedProducerCompanyNumber) &&
                                     migratedProducerCompanyNumber == companyNumber;
                             })))
                    {
                        result = RuleResult.Fail(
                            string.Format(@"We have previously issued a producer registration number (PRN) to {0} with company registration number (CRN) {1}. To register this producer, provide its existing PRN and use the status 'A' in the XML file.",
                                element.GetProducerName(), company.companyNumber),
                            Core.Shared.ErrorLevel.Error);
                    }
                }
            }

            return result;
        }
    }
}
