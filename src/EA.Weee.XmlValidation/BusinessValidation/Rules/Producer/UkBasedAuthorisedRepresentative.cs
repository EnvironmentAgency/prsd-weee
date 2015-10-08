﻿namespace EA.Weee.XmlValidation.BusinessValidation.Rules.Producer
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using BusinessValidation;
    using Xml;
    using Xml.Schemas;

    public class UkBasedAuthorisedRepresentative : IUkBasedAuthorisedRepresentative
    {
        private readonly IEnumerable<countryType> ukCountries = new List<countryType>
        {
            countryType.UKENGLAND,
            countryType.UKNORTHERNIRELAND,
            countryType.UKSCOTLAND,
            countryType.UKWALES
        };

        public RuleResult Evaluate(producerType producer)
        {
            if (producer.authorisedRepresentative.overseasProducer != null)
            {
                var producerBusinessItem = producer.producerBusiness.Item;
                contactDetailsContainerType officeContactDetails;

                if (producerBusinessItem is companyType)
                {
                    officeContactDetails = ((companyType)producerBusinessItem).registeredOffice;
                }
                else if (producerBusinessItem is partnershipType)
                {
                    officeContactDetails =
                        ((partnershipType)producerBusinessItem).principalPlaceOfBusiness;
                }
                else
                {
                    throw new ArgumentException(
                        string.Format("{0}: producerBusinessItem must be of type companyType or partnershipType",
                            producer.tradingName));
                }

                // abusing law of demeter here, but schema requires all these fields to be present and correct
                if (!ukCountries.Contains(officeContactDetails.contactDetails.address.country))
                {
                    return
                        RuleResult.Fail(
                            string.Format(
                                "You have entered {0} as an authorised representative with a non-UK address. Authorised representatives must be based in the UK. Review your file.",
                                producer.GetProducerName()));
                }
            }

            return RuleResult.Pass();
        }
    }
}