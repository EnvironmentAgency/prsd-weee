﻿namespace EA.Weee.RequestHandlers.DataReturns.CreateTestXmlFile
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Xml.Linq;
    using Domain.DataReturns;
    using EA.Prsd.Core.Mediator;
    using EA.Weee.Core.Shared;
    using EA.Weee.Requests.DataReturns;
    using Prsd.Core;

    public class CreateTestXmlFileHandler : IRequestHandler<CreateTestXmlFile, FileInfo>
    {
        private readonly IDataReturnContentsGenerator dataReturnContentsGenerator;
        private readonly IXmlGenerator xmlGenerator;

        public CreateTestXmlFileHandler(
            IDataReturnContentsGenerator dataReturnContentsGenerator,
            IXmlGenerator xmlGenerator)
        {
            this.dataReturnContentsGenerator = dataReturnContentsGenerator;
            this.xmlGenerator = xmlGenerator;
        }

        public async Task<FileInfo> HandleAsync(Requests.DataReturns.CreateTestXmlFile message)
        {
            DataReturnContents dataReturnContents = await dataReturnContentsGenerator.GenerateAsync(message.Settings);

            XDocument xml = xmlGenerator.GenerateXml(dataReturnContents);

            byte[] data;
            using (System.IO.MemoryStream stream = new System.IO.MemoryStream())
            {
                xml.Save(stream);
                data = stream.ToArray();
            }

            string fileName = string.Format(
                "PCS Data Return XML - {0:yyyy MM dd HH mm ss}.xml",
                SystemTime.UtcNow);

            return new FileInfo(fileName, data);
        }
    }
}
