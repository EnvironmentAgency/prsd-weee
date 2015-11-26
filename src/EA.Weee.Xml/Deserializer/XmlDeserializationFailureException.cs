﻿namespace EA.Weee.Xml.Deserializer
{
    using System;

    public class XmlDeserializationFailureException : Exception
    {
        private const string XmlDeserializationFailureMessage = "Unable to deserialize XML. See inner exception for details.";

        internal XmlDeserializationFailureException(Exception innerException)
            : base(XmlDeserializationFailureMessage, innerException)
        {
        }
    }
}