﻿namespace EA.Weee.XmlValidation.Tests.DataAccess.BusinessValidation.Rules.QuerySets
{
    using EA.Weee.Tests.Core.Model;
    using System;
    using XmlValidation.BusinessValidation.MemberRegistration.QuerySets;
    using Xunit;

    public class SchemeQuerySetTests
    {
        [Fact]
        public void GetSchemeApprovalNumber_SchemeIdDoesNotExist_ReturnsNull()
        {
            using (DatabaseWrapper database = new DatabaseWrapper())
            {
                // Arrange
                var schemeId = new Guid("15BE2DE7-8D51-470E-B27D-779AF14172AD");

                SchemeQuerySet schemeQuerySet = new SchemeQuerySet(database.WeeeContext);

                // Act
                string result = schemeQuerySet.GetSchemeApprovalNumber(schemeId);

                // Assert
                Assert.Null(result);
            }
        }

        [Fact]
        public void GetSchemeApprovalNumber_SchemeIdDoesExist_ReturnsApprovalNumber()
        {
            using (DatabaseWrapper database = new DatabaseWrapper())
            {
                ModelHelper helper = new ModelHelper(database.Model);

                // Arrange
                Scheme scheme1 = helper.CreateScheme();
                scheme1.ApprovalNumber = "ABC";
                
                database.Model.SaveChanges();

                SchemeQuerySet schemeQuerySet = new SchemeQuerySet(database.WeeeContext);

                // Act
                string result = schemeQuerySet.GetSchemeApprovalNumber(scheme1.Id);

                // Assert
                Assert.Equal("ABC", result);
            }
        }
    }
}
