﻿namespace EA.Weee.RequestHandlers.Tests.Unit
{
    using System;
    using System.Security;
    using System.Threading.Tasks;
    using EA.Weee.RequestHandlers.Security;
    using FakeItEasy;
    using RequestHandlers.DataReturns.ProcessDataReturnXmlFile;
    using Requests.DataReturns;
    using Weee.Tests.Core;
    using Xml.Converter;
    using Xunit;

    public class ProcessDataReturnsXMLFileHandlerTests
    {
        /// <summary>
        /// This test ensures that a user with no access to a scheme cannot create
        /// a data return.
        /// </summary>
        [Fact]
        public async void HandleAsync_UserNotAssociatedWithScheme_ThrowsSecurityException()
        {
            // Arrange
            IWeeeAuthorization authorization = new AuthorizationBuilder()
                .DenySchemeAccess()
                .Build();

            ProcessDataReturnsXmlFileHandler handler = new ProcessDataReturnsXmlFileHandler(
                                             A.Dummy<IProcessDataReturnXmlFileDataAccess>(),
                                             authorization,
                                             A.Dummy<IDataReturnsXmlValidator>(),
                                             A.Dummy<IXmlConverter>(),
                                             A.Dummy<IGenerateFromDataReturnsXml>());

            ProcessDataReturnsXMLFile message = A.Dummy<ProcessDataReturnsXMLFile>();
            // Act
            Func<Task<Guid>> testCode = async () => await handler.HandleAsync(message);

            // Assert
            await Assert.ThrowsAsync<SecurityException>(testCode);
        }
    }
}