﻿namespace EA.Weee.Sroc.Migration.OverrideImplementations
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using DataAccess;
    using Domain.Lookup;
    using RequestHandlers.Factories;
    using RequestHandlers.Scheme.MemberRegistration;

    public class MigrationProducerChargeCalculatorDataAccess : IMigrationProducerChargeCalculatorDataAccess
    {
        private readonly WeeeMigrationContext context;

        private Dictionary<ChargeBand, ChargeBandAmount> currentProducerChargeBandAmounts;
        private Dictionary<string, Dictionary<SchemeProducerYear, decimal>> sumOfExistingChargesLookup;

        public MigrationProducerChargeCalculatorDataAccess(WeeeMigrationContext context)
        {
            this.context = context;
            sumOfExistingChargesLookup = new Dictionary<string, Dictionary<SchemeProducerYear, decimal>>();
        }

        public ChargeBandAmount FetchCurrentChargeBandAmount(ChargeBand chargeBandType)
        {
            if (currentProducerChargeBandAmounts == null)
            {
                /* For now we only have one charge band amount for each type, so
                * we can fetch them all. When new charge band amounts are added,
                * this query will need to select only latest charge band amount
                * for each charge band type.
                */
                currentProducerChargeBandAmounts = context
                    .ChargeBandAmounts
                    .ToDictionary(pcb => pcb.ChargeBand, pcb => pcb);
            }

            return currentProducerChargeBandAmounts[chargeBandType];
        }

        public decimal FetchSumOfExistingCharges(string schemeApprovalNumber, string registrationNumber, int complianceYear)
        {
            throw new NotImplementedException();
        }

        public decimal FetchSumOfExistingChargesByDate(string schemeApprovalNumber, string registrationNumber, int complianceYear, DateTime submittedDate)
        {
            Dictionary<SchemeProducerYear, decimal> schemeProducerYear;
            if (!sumOfExistingChargesLookup.TryGetValue(schemeApprovalNumber, out schemeProducerYear))
            {
                schemeProducerYear = context
                    .ProducerSubmissions
                    .Where(p => p.MemberUpload.IsSubmitted && p.MemberUpload.CreatedDate < submittedDate)
                    .Where(p => p.RegisteredProducer.Scheme.ApprovalNumber == schemeApprovalNumber)
                    .GroupBy(p => new
                    {
                        RegistrationNumber = p.RegisteredProducer.ProducerRegistrationNumber,
                        ComplianceYear = p.RegisteredProducer.ComplianceYear
                    })
                    .Select(g => new
                    {
                        Key = g.Key,
                        Total = g.Sum(i => i.ChargeThisUpdate)
                    })
                    .ToDictionary(
                        g => new SchemeProducerYear(g.Key.RegistrationNumber, g.Key.ComplianceYear),
                        g => g.Total);

                sumOfExistingChargesLookup.Add(schemeApprovalNumber, schemeProducerYear);
            }

            SchemeProducerYear key = new SchemeProducerYear(registrationNumber, complianceYear);
            if (schemeProducerYear.ContainsKey(key))
            {
                return schemeProducerYear[key];
            }
            else
            {
                return 0;
            }
        }

        private struct SchemeProducerYear
        {
            public string RegistrationNumber { get; private set; }
            public int ComplianceYear { get; private set; }

            public SchemeProducerYear(string registrationNumber, int complianceYear)
                : this()
            {
                RegistrationNumber = registrationNumber;
                ComplianceYear = complianceYear;
            }
        }
    }
}