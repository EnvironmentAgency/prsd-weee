﻿namespace EA.Weee.RequestHandlers.Scheme.MemberRegistration
{
    using Domain.Error;
    using Domain.Scheme;
    using Interfaces;
    using Requests.Scheme.MemberRegistration;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Xml.Converter;
    using Xml.MemberRegistration;

    public class XmlChargeBandCalculator : IXMLChargeBandCalculator
    {
        private readonly IXmlConverter xmlConverter;
        private readonly IProducerChargeBandCalculatorChooser producerChargeCalculator;
        public List<MemberUploadError> ErrorsAndWarnings { get; set; }

        public XmlChargeBandCalculator(IXmlConverter xmlConverter, IProducerChargeBandCalculatorChooser producerChargeCalculator)
        {
            this.xmlConverter = xmlConverter;
            this.producerChargeCalculator = producerChargeCalculator;
            ErrorsAndWarnings = new List<MemberUploadError>();
        }

        public Dictionary<string, ProducerCharge> Calculate(ProcessXmlFile message)
        {
            var schemeType = xmlConverter.Deserialize<schemeType>(xmlConverter.Convert(message.Data));

            var producerCharges = new Dictionary<string, ProducerCharge>();
            var complianceYear = int.Parse(schemeType.complianceYear);

            foreach (var producer in schemeType.producerList)
            {
                var producerName = producer.GetProducerName();
                var producerCharge = Task.Run(() => producerChargeCalculator.GetProducerChargeBand(schemeType, producer)).Result;

                if (producerCharge != null)
                {
                    if (!producerCharges.ContainsKey(producerName))
                    {
                        producerCharges.Add(producerName, producerCharge);
                    }
                    else
                    {
                        ErrorsAndWarnings.Add(
                            new MemberUploadError(
                                ErrorLevel.Error,
                                UploadErrorType.Business,
                                string.Format(
                                    "We are unable to check for warnings associated with the charge band of the producer {0} until the duplicate name has been fixed.",
                                    producerName)));
                    }
                }
            }

            return producerCharges;
        }
    }
}
