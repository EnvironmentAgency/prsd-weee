﻿namespace EA.Weee.Core.PCS.MemberUploadTesting
{
    using System;

    public class ProducerListSettings : ISchemeBusinessSettings
    {
        public Guid OrganisationID { get; set; }

        public SchemaVersion SchemaVersion { get; set; }

        public int ComplianceYear { get; set; }

        public int NumberOfNewProducers { get; set; }

        public int NumberOfExistingProducers { get; set; }

        public bool IncludeMalformedSchema { get; set; }

        public bool IncludeUnexpectedFooElement { get; set; }

        public bool IgnoreStringLengthConditions { get; set; }
    }
}
