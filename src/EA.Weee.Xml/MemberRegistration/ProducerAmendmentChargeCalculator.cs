﻿namespace EA.Weee.Xml.MemberRegistration
{
    using EA.Weee.DataAccess.DataAccess;
    using System.Threading.Tasks;

    public class ProducerAmendmentChargeCalculator : IProducerChargeBandCalculator
    {
        private readonly IEnvironmentAgencyProducerChargeBandCalculator environmentAgencyProducerChargeBandCalculator;
        private readonly IRegisteredProducerDataAccess registeredProducerDataAccess;
        private readonly IFetchProducerCharge fetchProducerCharge;

        public ProducerAmendmentChargeCalculator(IEnvironmentAgencyProducerChargeBandCalculator environmentAgencyProducerChargeBandCalculator, IRegisteredProducerDataAccess registeredProducerDataAccess, IFetchProducerCharge fetchProducerCharge)
        {
            this.environmentAgencyProducerChargeBandCalculator = environmentAgencyProducerChargeBandCalculator;
            this.registeredProducerDataAccess = registeredProducerDataAccess;
            this.fetchProducerCharge = fetchProducerCharge;
        }

        public async Task<ProducerCharge> GetProducerChargeBand(schemeType schmemeType, producerType producerType)
        {
            var complianceYear = int.Parse(schmemeType.complianceYear);

            var previousProducerSubmission =
                await registeredProducerDataAccess.GetProducerRegistration(producerType.registrationNo, complianceYear, schmemeType.approvalNo);

            var previousAmendmentCharge = registeredProducerDataAccess.HasPreviousAmendmentCharge(producerType.registrationNo, complianceYear, schmemeType.approvalNo);

            var chargeband = await environmentAgencyProducerChargeBandCalculator.GetProducerChargeBand(schmemeType, producerType);

            if (previousProducerSubmission != null && previousProducerSubmission.CurrentSubmission != null)
            {
                if (!previousAmendmentCharge && (producerType.eeePlacedOnMarketBand == eeePlacedOnMarketBandType.Morethanorequalto5TEEEplacedonmarket &&
                                                 previousProducerSubmission.CurrentSubmission.EEEPlacedOnMarketBandType == (int)eeePlacedOnMarketBandType.Lessthan5TEEEplacedonmarket))
                {
                    return chargeband;
                }
            }

            return new ProducerCharge()
            {
                ChargeBandAmount = chargeband.ChargeBandAmount,
                Amount = 0
            };
        }

        public bool IsMatch(schemeType scheme, producerType producer)
        {
            var year = int.Parse(scheme.complianceYear);
            var previousProducerSubmission = Task.Run(() => registeredProducerDataAccess.GetProducerRegistration(producer.registrationNo, year, scheme.approvalNo)).Result;

            return producer.status == statusType.A && (previousProducerSubmission != null);
        }
    }
}
