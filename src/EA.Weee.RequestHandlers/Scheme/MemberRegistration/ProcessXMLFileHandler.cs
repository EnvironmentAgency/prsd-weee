﻿namespace EA.Weee.RequestHandlers.Scheme.MemberRegistration
{
    using DataAccess;
    using DataAccess.DataAccess;
    using Domain.Error;
    using Domain.Producer;
    using Domain.Scheme;
    using EA.Weee.Core.Shared;
    using EA.Weee.RequestHandlers.Security;
    using EA.Weee.Xml.MemberRegistration;
    using Interfaces;
    using Prsd.Core.Mediator;
    using Requests.Scheme.MemberRegistration;
    using System;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading.Tasks;
    using Xml.Converter;
    using ErrorLevel = Domain.Error.ErrorLevel;

    internal class ProcessXMLFileHandler : IRequestHandler<ProcessXmlFile, Guid>
    {
        private readonly WeeeContext context;
        private readonly IWeeeAuthorization authorization;
        private readonly IXMLValidator xmlValidator;
        private readonly IXmlConverter xmlConverter;
        private readonly IGenerateFromXml generateFromXml;
        private readonly IXMLChargeBandCalculator xmlChargeBandCalculator;
        private readonly IProducerSubmissionDataAccess producerSubmissionDataAccess;
        private readonly ITotalChargeCalculator totalChargeCalculator;
        private readonly ITotalChargeCalculatorDataAccess totalChargeCalculatorDataAccess;

        public ProcessXMLFileHandler(WeeeContext context, IWeeeAuthorization authorization,
            IXMLValidator xmlValidator, IGenerateFromXml generateFromXml, IXmlConverter xmlConverter,
            IXMLChargeBandCalculator xmlChargeBandCalculator, IProducerSubmissionDataAccess producerSubmissionDataAccess, ITotalChargeCalculator totalChargeCalculator, ITotalChargeCalculatorDataAccess totalChargeCalculatorDataAccess)
        {
            this.context = context;
            this.authorization = authorization;
            this.xmlValidator = xmlValidator;
            this.xmlConverter = xmlConverter;
            this.xmlChargeBandCalculator = xmlChargeBandCalculator;
            this.generateFromXml = generateFromXml;
            this.producerSubmissionDataAccess = producerSubmissionDataAccess;
            this.totalChargeCalculator = totalChargeCalculator;
            this.totalChargeCalculatorDataAccess = totalChargeCalculatorDataAccess;
        }

        public async Task<Guid> HandleAsync(ProcessXmlFile message)
        {
            authorization.EnsureOrganisationAccess(message.OrganisationId);

            // record XML processing start time
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            var errors = await xmlValidator.Validate(message);

            List<MemberUploadError> memberUploadErrors = errors as List<MemberUploadError> ?? errors.ToList();
            bool containsSchemaErrors = memberUploadErrors.Any(e => e.ErrorType == UploadErrorType.Schema);
            bool containsErrorOrFatal = memberUploadErrors.Any(e => (e.ErrorLevel == ErrorLevel.Error || e.ErrorLevel == ErrorLevel.Fatal));

            Dictionary<string, ProducerCharge> producerCharges = null;
            int deserializedcomplianceYear = 0;

            decimal? totalChargesCalculated = 0;

            var scheme = await context.Schemes.SingleAsync(c => c.OrganisationId == message.OrganisationId);
            var annualChargeToBeAdded = false;

            if (!containsSchemaErrors || !containsErrorOrFatal)
            {
                var deserializedXml = xmlConverter.Deserialize<schemeType>(xmlConverter.Convert(message.Data));
                deserializedcomplianceYear = int.Parse(deserializedXml.complianceYear);

                var hasAnnualCharge = totalChargeCalculatorDataAccess.CheckSchemeHasAnnualCharge(scheme, deserializedcomplianceYear);

                if (!hasAnnualCharge)
                {
                    var annualcharge = scheme.CompetentAuthority.AnnualChargeAmount ?? 0;
                    if (annualcharge > 0 || scheme.CompetentAuthority.Abbreviation == UKCompetentAuthorityAbbreviationType.EA)
                    {
                        annualChargeToBeAdded = true;
                    }
                }

                producerCharges = totalChargeCalculator.TotalCalculatedCharges(message, scheme, deserializedcomplianceYear, annualChargeToBeAdded, ref totalChargesCalculated);

                if (xmlChargeBandCalculator.ErrorsAndWarnings.Any(e => e.ErrorLevel == ErrorLevel.Error)
                    && memberUploadErrors.All(e => e.ErrorLevel != ErrorLevel.Error))
                {
                    throw new ApplicationException(String.Format(
                        "Upload for Organisation '{0}' has no validation errors, but does have producer charge calculation errors which are not currently being enforced",
                        message.OrganisationId));
                }
            }

            var totalCharges = totalChargesCalculated ?? 0;

            var upload = generateFromXml.GenerateMemberUpload(message, memberUploadErrors, totalCharges, scheme, annualChargeToBeAdded);
            IEnumerable<ProducerSubmission> producers = Enumerable.Empty<ProducerSubmission>();

            //Build producers domain object if there are no errors (schema or business) during validation of xml file.
            if (!containsErrorOrFatal)
            {
                producers = await generateFromXml.GenerateProducers(message, upload, producerCharges);
            }

            // record XML processing end time
            stopwatch.Stop();
            upload.SetProcessTime(stopwatch.Elapsed);

            context.MemberUploads.Add(upload);
            producerSubmissionDataAccess.AddRange(producers);

            await context.SaveChangesAsync();
            return upload.Id;
        }
    }
}
