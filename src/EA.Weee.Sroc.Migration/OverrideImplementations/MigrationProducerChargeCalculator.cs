﻿namespace EA.Weee.Sroc.Migration.OverrideImplementations
{
    using System;
    using Domain.Lookup;
    using RequestHandlers.Scheme.MemberRegistration;
    using Xml.MemberRegistration;

    public class MigrationProducerChargeCalculator : IMigrationProducerChargeCalculator
    {
        private readonly IMigrationProducerChargeCalculatorDataAccess dataAccess;
        private readonly IProducerChargeBandCalculatorChooser producerChargeBandCalculatorChooser;

        public MigrationProducerChargeCalculator(IMigrationProducerChargeCalculatorDataAccess dataAccess, 
            IProducerChargeBandCalculator producerChargeBandCalculator, 
            IProducerChargeBandCalculatorChooser producerChargeBandCalculatorChooser)
        {
            this.dataAccess = dataAccess;
            this.producerChargeBandCalculatorChooser = producerChargeBandCalculatorChooser;
        }

        public ProducerCharge CalculateCharge(schemeType scheme, producerType producer, int complianceYear)
        {
            throw new NotImplementedException();
        }

        public ProducerCharge CalculateCharge(schemeType scheme, producerType producer, int complianceYear, DateTime submittedDate)
        {
            ProducerCharge producerCharge = GetProducerCharge(scheme, producer, complianceYear);

            //if (producer.status == statusType.A)
            //{
            //    decimal sumOfExistingCharges = dataAccess.FetchSumOfExistingChargesByDate(scheme.approvalNo, producer.registrationNo, complianceYear, submittedDate);

            //    producerCharge.Amount = Math.Max(0, producerCharge.ChargeBandAmount.Amount - sumOfExistingCharges);
            //}

            return producerCharge;
        }

        private ProducerCharge GetProducerCharge(schemeType scheme, producerType producer, int complianceYear)
        {
            var calculator = producerChargeBandCalculatorChooser.GetCalculator(scheme, producer, complianceYear);

            ChargeBand chargeBandType = calculator.GetProducerChargeBand(producer);

            ChargeBandAmount currentChargeBandAmount =
                dataAccess.FetchCurrentChargeBandAmount(chargeBandType);

            return new ProducerCharge()
            {
                ChargeBandAmount = currentChargeBandAmount,
                Amount = currentChargeBandAmount.Amount
            };
        }
    }
}