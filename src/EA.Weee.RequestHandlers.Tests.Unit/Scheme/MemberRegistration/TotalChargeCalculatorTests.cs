﻿namespace EA.Weee.RequestHandlers.Tests.Unit.Scheme.MemberRegistration
{
    using System;
    using Domain;
    using Domain.Scheme;
    using EA.Weee.Domain.Lookup;
    using EA.Weee.RequestHandlers.Scheme.Interfaces;
    using EA.Weee.Requests.Scheme.MemberRegistration;
    using EA.Weee.Xml.Deserialization;
    using EA.Weee.Xml.MemberRegistration;
    using FakeItEasy;
    using RequestHandlers.Scheme.MemberRegistration;
    using RequestHandlers.Security;
    using System.Collections.Generic;
    using System.IO;
    using System.Reflection;
    using System.Text;
    using Xml.Converter;
    using Xunit;

    public class TotalChargeCalculatorTests
    {
        private readonly IXMLChargeBandCalculator xmlChargeBandCalculator;
        private readonly ITotalChargeCalculatorDataAccess totalChargeCalculatorDataAccess;
        private readonly IProducerChargeCalculator producerChargeCalculator;
        private readonly TotalChargeCalculator totalChargeCalculator;
        private readonly IWeeeAuthorization authorization;
        private bool hasAnnualCharge;
        private decimal? totalCharge;
        private readonly ProcessXmlFile file;
        private readonly IXmlConverter xmlConverter;
        private readonly IProducerChargeCalculatorDataAccess producerChargeCalculatorDataAccess;
        private readonly IProducerChargeBandCalculator producerChargeBandCalculator;

        public TotalChargeCalculatorTests()
        {
            xmlChargeBandCalculator = A.Fake<IXMLChargeBandCalculator>();
            totalChargeCalculatorDataAccess = A.Fake<ITotalChargeCalculatorDataAccess>();
            producerChargeCalculator = A.Fake<IProducerChargeCalculator>();
            producerChargeCalculatorDataAccess = A.Fake<IProducerChargeCalculatorDataAccess>();
            producerChargeBandCalculator = A.Fake<IProducerChargeBandCalculator>();

            totalCharge = 0;
            authorization = A.Fake<IWeeeAuthorization>();
            file = ProcessTestXmlFile();

            totalChargeCalculator = new TotalChargeCalculator(xmlChargeBandCalculator, authorization, totalChargeCalculatorDataAccess);
            producerChargeCalculator = new ProducerChargeCalculator(producerChargeCalculatorDataAccess, producerChargeBandCalculator);
        }

        [Fact]
        public void TotalCalculatedCharges_GivenXMLFile_CalculateXMLChargeBand()
        {
            totalChargeCalculator.TotalCalculatedCharges(file, Scheme(), 2019, ref hasAnnualCharge, ref totalCharge);

            A.CallTo(() => xmlChargeBandCalculator.Calculate(file)).MustHaveHappened(Repeated.Exactly.Once);
        }

        [Fact]
        public void TotalCalculatedCharges_GivenSchemeHasAnnualChargeForComplianceYear_TotalShouldNotContainAnnualCharge()
        {
            var scheme = Scheme();
            A.CallTo(() => totalChargeCalculatorDataAccess.CheckSchemeHasAnnualCharge(scheme, 2019)).Returns(true);
            hasAnnualCharge = false;
            totalCharge = 0;

            var result = totalChargeCalculator.TotalCalculatedCharges(file, scheme, 2019, ref hasAnnualCharge, ref totalCharge);

            Assert.Equal(totalCharge, 0);
            Assert.Equal(hasAnnualCharge, true);
        }

        [Fact]
        public void TotalCalculatedCharges_GivenSchemeDoesNotHaveAnnualChargeForComplianceYear_TotalShouldContainAnnualCharge()
        {
            var scheme = Scheme();
            A.CallTo(() => totalChargeCalculatorDataAccess.CheckSchemeHasAnnualCharge(scheme, 2019)).Returns(false);
            hasAnnualCharge = true;
            totalCharge = 0;

            var result = totalChargeCalculator.TotalCalculatedCharges(file, scheme, 2019, ref hasAnnualCharge, ref totalCharge);

            Assert.Equal(totalCharge, 100);
            Assert.Equal(hasAnnualCharge, true);
        }

        [Fact]
        public void TotalCalculatedCharges_GivenProducerChargesAndNoAnnualCharge_TotalShouldBeCalculated()
        {
            var producerCharges = ProducerCharges();

            A.CallTo(() => xmlChargeBandCalculator.Calculate(file)).Returns(producerCharges);
            A.CallTo(() => totalChargeCalculatorDataAccess.CheckSchemeHasAnnualCharge(A<Scheme>._, A<int>._)).Returns(true);

            var result = totalChargeCalculator.TotalCalculatedCharges(file, Scheme(), 2019, ref hasAnnualCharge, ref totalCharge);

            Assert.Equal(300, totalCharge);
        }

        [Fact]
        public void TotalCalculatedCharges_GivenProducerChargesAndAnnualCharge_TotalShouldBeCalculated()
        {
            var producerCharges = ProducerCharges();

            A.CallTo(() => xmlChargeBandCalculator.Calculate(file)).Returns(producerCharges);
            A.CallTo(() => totalChargeCalculatorDataAccess.CheckSchemeHasAnnualCharge(A<Scheme>._, A<int>._)).Returns(false);

            var result = totalChargeCalculator.TotalCalculatedCharges(file, Scheme(), 2019, ref hasAnnualCharge, ref totalCharge);

            Assert.Equal(400, totalCharge);
        }

        [Fact]
        public void TotalCalculatedCharges_GivenProducerCharges_ProducerChargesShouldBeReturned()
        {
            var producerCharges = ProducerCharges();

            A.CallTo(() => xmlChargeBandCalculator.Calculate(file)).Returns(producerCharges);

            var result = totalChargeCalculator.TotalCalculatedCharges(file, Scheme(), 2019, ref hasAnnualCharge, ref totalCharge);
            
            Assert.Equal(producerCharges, result);
        }

        private Scheme Scheme()
        {
            var scheme = A.Fake<Scheme>();
            var competantAuthority = A.Fake<UKCompetentAuthority>();
            A.CallTo(() => competantAuthority.AnnualChargeAmount).Returns(100);
            A.CallTo(() => scheme.CompetentAuthority).Returns(competantAuthority);
            return scheme;
        }

        private static ProcessXmlFile ProcessTestXmlFile()
        {
            string absoluteFilePath = Path.Combine(
               Path.GetDirectoryName(Assembly.GetExecutingAssembly().CodeBase),
               @"ExampleXML\v3-valid-ChargeBand.xml");
            byte[] xml = Encoding.ASCII.GetBytes(File.ReadAllText(new Uri(absoluteFilePath).LocalPath));
            ProcessXmlFile request = new ProcessXmlFile(A.Dummy<Guid>(), xml, "File name");
            return request;
        }

        private XMLChargeBandCalculator XmlChargeBandCalculator()
        {
            var xmlConverter = new XmlConverter(A.Fake<IWhiteSpaceCollapser>(), new Deserializer());

            IProducerChargeCalculator producerChargerCalculator = null;
            return new XMLChargeBandCalculator(xmlConverter, producerChargerCalculator);
        }

        private Dictionary<string, ProducerCharge> ProducerCharges()
        {
            var producerCharges = new Dictionary<string, ProducerCharge>
            {
                { "1", new ProducerCharge() { Amount = 100 } },
                { "2", new ProducerCharge() { Amount = 200 } }
            };
            return producerCharges;   
        }
    }
}