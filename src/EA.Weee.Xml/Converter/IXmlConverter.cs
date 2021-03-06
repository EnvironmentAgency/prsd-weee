﻿namespace EA.Weee.Xml.Converter
{
    using System.Xml.Linq;

    public interface IXmlConverter
    {
        XDocument Convert(byte[] data);

        string XmlToUtf8String(byte[] data);

        T Deserialize<T>(XDocument xdoc);
    }
}
