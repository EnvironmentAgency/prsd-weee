﻿//namespace EA.Weee.RequestHandlers.Tests.Unit.Scheme.MemberRegistration.XmlValidation.BusinessValidation
//{
//    using System;
//    using System.Collections.Generic;
//    using System.Linq;
//    using Core.Helpers;
//    using Core.XmlBusinessValidation;
//    using DataAccess;
//    using Domain;
//    using Domain.Organisation;
//    using Domain.Producer;
//    using Domain.Scheme;
//    using FakeItEasy;
//    using FluentValidation;
//    using FluentValidation.Internal;
//    using Helpers;
//    using RequestHandlers;
//    using RequestHandlers.Scheme.MemberRegistration.XmlValidation.BusinessValidation;
//    using RequestHandlers.Scheme.MemberRegistration.XmlValidation.BusinessValidation.RuleEvaluators;
//    using RequestHandlers.Scheme.MemberRegistration.XmlValidation.BusinessValidation.Rules;
//    using Xunit;
//    using ValidationContext = XmlValidation.ValidationContext;

//    public class SchemeTypeValidatorTests
//    {
//        private readonly IRuleSelector ruleSelector;

//        public SchemeTypeValidatorTests()
//        {
//            ruleSelector = A.Fake<IRuleSelector>();

//            // By default, rules pass
//            A.CallTo(() => ruleSelector.EvaluateRule(A<ProducerNameWarning>._))
//                .Returns(RuleResult.Pass());

//            A.CallTo(() => ruleSelector.EvaluateRule(A<ProducerNameRegisteredBefore>._))
//              .Returns(RuleResult.Pass());

//            A.CallTo(() => ruleSelector.EvaluateRule(A<ProducerNameRegisteredBefore>._))
//                .Returns(RuleResult.Pass());
//        }

//        [Fact]
//        public void
//            SetOfDuplicateRegistrationNumbers_ValidationFails_IncludesRegistraionNumberInMessage_AndErrorLevelIsError()
//        {
//            const string registrationNumber = "ABC12345";
//            var xml = new schemeType
//            {
//                producerList = ProducersWithRegistrationNumbers(registrationNumber, registrationNumber)
//            };

//            var result = SchemeTypeValidator()
//                .Validate(xml,
//                    new RulesetValidatorSelector(
//                        RequestHandlers.Scheme.MemberRegistration.XmlValidation.BusinessValidation.SchemeTypeValidator
//                            .NonDataValidation));

//            Assert.False(result.IsValid);
//            Assert.Contains(registrationNumber, result.Errors.Single().ErrorMessage);
//            Assert.Equal(ErrorLevel.Error, result.Errors.Single().CustomState);
//        }

//        [Fact]
//        public void SetOfEmptyRegistrationNumbers_ValidationSucceeds()
//        {
//            var xml = new schemeType
//            {
//                producerList = ProducersWithRegistrationNumbers(string.Empty, string.Empty)
//            };

//            var result = SchemeTypeValidator().Validate(xml);

//            Assert.True(result.IsValid);
//        }

//        [Fact]
//        public void TwoSetsOfDuplicateRegistrationNumbers_ValidationFails_IncludesBothRegistrationNumbersInMessages()
//        {
//            const string firstRegistrationNumber = "ABC12345";
//            const string secondRegistrationNumber = "XYZ54321";
//            var xml = new schemeType
//            {
//                producerList =
//                    ProducersWithRegistrationNumbers(firstRegistrationNumber, firstRegistrationNumber,
//                        secondRegistrationNumber, secondRegistrationNumber)
//            };

//            var result = SchemeTypeValidator()
//                .Validate(xml,
//                    new RulesetValidatorSelector(
//                        RequestHandlers.Scheme.MemberRegistration.XmlValidation.BusinessValidation.SchemeTypeValidator
//                            .NonDataValidation));

//            Assert.False(result.IsValid);

//            var aggregatedErrorMessages =
//                result.Errors.Select(err => err.ErrorMessage).Aggregate((curr, next) => curr + ", " + next);

//            Assert.Contains(firstRegistrationNumber, aggregatedErrorMessages);
//            Assert.Contains(secondRegistrationNumber, aggregatedErrorMessages);
//        }

//        [Fact]
//        public void TwoProducersWithDifferentRegistrationNumbers_ValidationSucceeds()
//        {
//            var xml = new schemeType
//            {
//                producerList = ProducersWithRegistrationNumbers("ABC12345", "XYZ54321").ToArray()
//            };

//            var result = SchemeTypeValidator()
//                .Validate(xml,
//                    new RulesetValidatorSelector(
//                        RequestHandlers.Scheme.MemberRegistration.XmlValidation.BusinessValidation.SchemeTypeValidator
//                            .NonDataValidation));

//            Assert.True(result.IsValid);
//        }

//        [Fact]
//        public void ProducerWithoutProducerName_ThrowsArgumentException()
//        {
//            const string producerName = null;
//            var xml = new schemeType
//            {
//                producerList = ProducersWithProducerNames(producerName)
//            };

//            Assert.Throws<ArgumentException>(() => SchemeTypeValidator()
//                .Validate(xml,
//                    new RulesetValidatorSelector(
//                        RequestHandlers.Scheme.MemberRegistration.XmlValidation.BusinessValidation.SchemeTypeValidator
//                            .NonDataValidation)));
//        }

//        [Fact]
//        public void ProducerWithEmptyProducerName_ThrowsArgumentException()
//        {
//            var producerName = string.Empty;
//            var xml = new schemeType
//            {
//                producerList = ProducersWithProducerNames(producerName)
//            };

//            Assert.Throws<ArgumentException>(() => SchemeTypeValidator()
//                .Validate(xml,
//                    new RulesetValidatorSelector(
//                        RequestHandlers.Scheme.MemberRegistration.XmlValidation.BusinessValidation.SchemeTypeValidator
//                            .NonDataValidation)));
//        }

//        [Fact]
//        public void SetOfDuplicateProducerNames_ValidationFails_IncludesProducerNameInMessage_AndErrorLevelIsError()
//        {
//            const string producerName = "Producer Name";
//            var xml = new schemeType
//            {
//                producerList = ProducersWithProducerNames(producerName, producerName)
//            };

//            var result = SchemeTypeValidator()
//                .Validate(xml,
//                    new RulesetValidatorSelector(
//                        RequestHandlers.Scheme.MemberRegistration.XmlValidation.BusinessValidation.SchemeTypeValidator
//                            .NonDataValidation));

//            Assert.False(result.IsValid);
//            Assert.Contains(producerName, result.Errors.Single().ErrorMessage);
//            Assert.Equal(ErrorLevel.Error, result.Errors.Single().CustomState);
//        }

//        [Fact]
//        public void TwoSetsOfDuplicateProducerNames_ValidationFails_IncludesBothProducerNamesInMessages()
//        {
//            const string firstProducerName = "First Producer Name";
//            const string secondProducerName = "Second Producer Name";
//            var xml = new schemeType
//            {
//                producerList =
//                    ProducersWithProducerNames(firstProducerName, firstProducerName, secondProducerName,
//                        secondProducerName)
//            };

//            var result = SchemeTypeValidator()
//                .Validate(xml,
//                    new RulesetValidatorSelector(
//                        RequestHandlers.Scheme.MemberRegistration.XmlValidation.BusinessValidation.SchemeTypeValidator
//                            .NonDataValidation));

//            Assert.False(result.IsValid);

//            var aggregatedErrorMessages =
//                result.Errors.Select(err => err.ErrorMessage).Aggregate((curr, next) => curr + ", " + next);

//            Assert.Contains(firstProducerName, aggregatedErrorMessages);
//            Assert.Contains(secondProducerName, aggregatedErrorMessages);
//        }

//        [Fact]
//        public void TwoProducersWithDifferentProducerNames_ValidationSucceeds()
//        {
//            var xml = new schemeType
//            {
//                producerList = ProducersWithProducerNames("First Producer Name", "Second Producer Name")
//            };

//            var result = SchemeTypeValidator()
//                .Validate(xml,
//                    new RulesetValidatorSelector(
//                        RequestHandlers.Scheme.MemberRegistration.XmlValidation.BusinessValidation.SchemeTypeValidator
//                            .NonDataValidation));

//            Assert.True(result.IsValid);
//        }

//        [Fact]
//        public void SchemeApprovalNumber_MatchesApprovalNumberinXML_ValidationSucceeds()
//        {
//            var xml = new schemeType
//            {
//                approvalNo = "Test Approval Number 1",
//                producerList = new[]
//                {
//                    new producerType
//                    {
//                        obligationType = obligationTypeType.B2B
//                    }
//                }
//            };
//            var orgId = new Guid("20C569E6-C4A0-40C2-9D87-120906D5434B");
//            var existingScheme = new Scheme(orgId);
//            existingScheme.UpdateScheme("test", "Test Approval Number 1", string.Empty, ObligationType.B2B, Guid.NewGuid());

//            var result = SchemeTypeValidator(existingScheme, orgId)
//               .Validate(xml,
//                   new RulesetValidatorSelector(
//                       RequestHandlers.Scheme.MemberRegistration.XmlValidation.BusinessValidation.SchemeTypeValidator
//                           .DataValidation));

//            Assert.True(result.IsValid);
//        }

//        [Fact]
//        public void SchemeApprovalNumber_DoesNotMatchApprovalNumberinXML_ValidationFails()
//        {
//            var xml = new schemeType
//            {
//                complianceYear = "2016",
//                approvalNo = "Test Approval Number 2",
//                producerList = new[]
//                {
//                    new producerType
//                    {
//                        registrationNo = "ABC12345",
//                        obligationType = obligationTypeType.B2B
//                    }
//                }
//            };
//            var orgId = new Guid("20C569E6-C4A0-40C2-9D87-120906D5434B");
//            var existingScheme = new Scheme(orgId);
//            existingScheme.UpdateScheme("test", "Test Approval Number 1", String.Empty, ObligationType.B2B, Guid.NewGuid());
            
//            var result = SchemeTypeValidator(existingScheme, orgId)
//               .Validate(xml,
//                   new RulesetValidatorSelector(
//                       RequestHandlers.Scheme.MemberRegistration.XmlValidation.BusinessValidation.SchemeTypeValidator
//                           .DataValidation));

//            Assert.False(result.IsValid);
//        }

//        [Fact]
//        public void ShouldEvaluateProducerAlreadyRegisteredRule()
//        {
//            var xml = new schemeType
//            {
//                complianceYear = "2016",
//                producerList = new[]
//                {
//                    new producerType
//                    {
//                        registrationNo = "ABC12345",
//                        status = statusType.A,
//                        obligationType = obligationTypeType.B2B,

//                        producerBusiness = new producerBusinessType
//                        {
//                            Item = new partnershipType
//                            {
//                                partnershipName = "Test Name"
//                            }
//                        }
//                    }
//                }
//            };

//            SchemeTypeValidator().Validate(xml, new RulesetValidatorSelector(BusinessValidator.CustomRules));

//            A.CallTo(() => ruleSelector.EvaluateRule(A<ProducerAlreadyRegisteredError>._))
//                .MustHaveHappened(Repeated.Exactly.Once);
//        }

//        [Fact]
//        public void ShouldEvaluateProducerNameWarningRule()
//        {
//            var xml = new schemeType
//            {
//                producerList = new[]
//                {
//                    new producerType
//                    {
//                        registrationNo = "ABC12345",
//                        producerBusiness = new producerBusinessType
//                        {
//                            Item = new partnershipType
//                            {
//                                partnershipName = "Test Name"
//                            }
//                        }
//                    }
//                }
//            };

//            SchemeTypeValidator().Validate(xml, new RulesetValidatorSelector(BusinessValidator.CustomRules));

//            A.CallTo(() => ruleSelector.EvaluateRule(A<ProducerNameWarning>._))
//                .MustHaveHappened(Repeated.Exactly.Once);
//        }

//        [Theory]
//        [InlineData(Core.Shared.ErrorLevel.Warning)]
//        [InlineData(Core.Shared.ErrorLevel.Error)]
//        public void EvaluteProducerNameWarningRuleFails_ShouldMapErrorMessage_AndErrorLevel(Core.Shared.ErrorLevel errorLevel)
//        {
//            var errorMessage = "Some sort of error";
//            var failure = RuleResult.Fail(errorMessage, errorLevel);

//            A.CallTo(() => ruleSelector.EvaluateRule(A<ProducerNameWarning>._))
//                .Returns(failure);

//            var xml = new schemeType
//            {
//                producerList = new[]
//                {
//                    new producerType
//                    {
//                        registrationNo = "ABC12345",
//                        producerBusiness = new producerBusinessType
//                        {
//                            Item = new partnershipType
//                            {
//                                partnershipName = "Test Name"
//                            }
//                        }
//                    }
//                }
//            };

//            var result = SchemeTypeValidator().Validate(xml, new RulesetValidatorSelector(BusinessValidator.CustomRules));

//            Assert.Single(result.Errors);

//            var error = result.Errors.Single();

//            Assert.Equal(errorLevel.ToDomainEnumeration<ErrorLevel>(), error.CustomState);
//            Assert.Equal(errorMessage, error.ErrorMessage);
//        }

//        [Fact]
//        public void ProducerNameHasNotBeenRegisteredBefore_ReturnsValidResult()
//        {
//            A.CallTo(() => ruleSelector.EvaluateRule(A<ProducerNameRegisteredBefore>._))
//                .Returns(RuleResult.Pass());

//            var scheme = new schemeType
//            {
//                producerList = new[]
//                {
//                    new producerType()
//                }
//            };

//            var result = SchemeTypeValidator().Validate(scheme, new RulesetValidatorSelector(BusinessValidator.CustomRules));

//            A.CallTo(() => ruleSelector.EvaluateRule(A<ProducerNameRegisteredBefore>._))
//                .MustHaveHappened(Repeated.Exactly.Once);

//            Assert.True(result.IsValid);
//        }

//        [Theory]
//        [InlineData(Core.Shared.ErrorLevel.Warning)]
//        [InlineData(Core.Shared.ErrorLevel.Error)]
//        public void ProducerNameHasBeenRegisteredBefore_ReturnsResult_WithMappedState_AndMappedErrorMessage(Core.Shared.ErrorLevel errorLevel)
//        {
//            var ruleResult = RuleResult.Fail("oops", errorLevel);

//            A.CallTo(() => ruleSelector.EvaluateRule(A<ProducerNameRegisteredBefore>._))
//                .Returns(ruleResult);

//            var scheme = new schemeType
//            {
//                producerList = new[]
//                {
//                    new producerType()
//                }
//            };

//            var result = SchemeTypeValidator().Validate(scheme, new RulesetValidatorSelector(BusinessValidator.CustomRules));

//            A.CallTo(() => ruleSelector.EvaluateRule(A<ProducerNameRegisteredBefore>._))
//                .MustHaveHappened(Repeated.Exactly.Once);

//            Assert.False(result.IsValid);
//            Assert.Equal(ruleResult.Message, result.Errors.Single().ErrorMessage);
//            Assert.Equal(errorLevel.ToDomainEnumeration<ErrorLevel>(), result.Errors.Single().CustomState);
//        }

//        private IValidator<schemeType> SchemeTypeValidator(Guid? existingOrganisationId = null,
//            Guid? organisationId = null)
//        {
//            return SchemeTypeValidator(new Scheme(existingOrganisationId ?? Guid.NewGuid()), organisationId);
//        }

//        private IValidator<schemeType> SchemeTypeValidator(Scheme scheme, Guid? organisationId = null)
//        {
//            return new SchemeTypeValidator(ValidationContext.Create(scheme), organisationId ?? Guid.NewGuid(), ruleSelector);
//        }

//        private producerType[] ProducersWithRegistrationNumbers(params string[] regstrationNumbers)
//        {
//            return regstrationNumbers.Select(r => new producerType
//            {
//                status = statusType.A,
//                registrationNo = r,
//                producerBusiness = new producerBusinessType
//                {
//                    Item = new partnershipType
//                    {
//                        partnershipName = Guid.NewGuid().ToString()
//                    }
//                }
//            }).ToArray();
//        }

//        private producerType[] ProducersWithProducerNames(params string[] producerNames)
//        {
//            return producerNames.Select(n => new producerType
//            {
//                status = statusType.I,
//                producerBusiness = new producerBusinessType
//                {
//                    Item = new partnershipType
//                    {
//                        partnershipName = n
//                    }
//                }
//            }).ToArray();
//        }

//        private ObligationType MapObligationType(obligationTypeType obligationType)
//        {
//            switch (obligationType)
//            {
//                case obligationTypeType.B2B:
//                    return ObligationType.B2B;
//                case obligationTypeType.B2C:
//                    return ObligationType.B2C;

//                default:
//                    return ObligationType.Both;
//            }
//        }

//        private readonly DbContextHelper dbContextHelper = new DbContextHelper();

//        /// <summary>
//        ///     Sets up a faked WeeeContext with 2 schemes
//        /// </summary>
//        /// <returns></returns>
//        private WeeeContext CreateFakeDatabase()
//        {
//            var memberUpload1 = A.Fake<MemberUpload>();
//            A.CallTo(() => memberUpload1.OrganisationId).Returns(new Guid("20C569E6-C4A0-40C2-9D87-120906D5434B"));
//            A.CallTo(() => memberUpload1.ComplianceYear).Returns(2016);
//            A.CallTo(() => memberUpload1.IsSubmitted).Returns(true);

//            var memberUpload2 = A.Fake<MemberUpload>();
//            A.CallTo(() => memberUpload2.OrganisationId).Returns(new Guid("20C569E6-C4A0-40C2-9D87-120906D5434B"));
//            A.CallTo(() => memberUpload2.ComplianceYear).Returns(2017);
//            A.CallTo(() => memberUpload2.IsSubmitted).Returns(true);

//            Producer producer1 = FakeProducer.Create(MapObligationType(obligationTypeType.B2B), "ABC", false);

//            Producer producer2 = FakeProducer.Create(MapObligationType(obligationTypeType.B2C), "ABC", true);

//            var organisation1 = A.Fake<Organisation>();
//            A.CallTo(() => organisation1.TradingName).Returns("Test Trading Name 1");

//            var organisation2 = A.Fake<Organisation>();
//            A.CallTo(() => organisation2.TradingName).Returns("Test Trading Name 2");

//            var scheme1 = A.Fake<Scheme>();
//            A.CallTo(() => scheme1.OrganisationId).Returns(new Guid("20C569E6-C4A0-40C2-9D87-120906D5434B"));
//            A.CallTo(() => scheme1.ApprovalNumber).Returns("Test Approval Number 1");

//            var scheme2 = A.Fake<Scheme>();
//            A.CallTo(() => scheme2.OrganisationId).Returns(new Guid("A99F0A92-E08D-47F0-9758-82F02DB816FA"));
//            A.CallTo(() => scheme2.ApprovalNumber).Returns("Test Approval Number 2");

//            // Wire up scheme to organisations (1-way).
//            A.CallTo(() => scheme1.Organisation).Returns(organisation1);
//            A.CallTo(() => scheme2.Organisation).Returns(organisation2);

//            // Wire up member uploads to organisations (1-way)
//            A.CallTo(() => memberUpload1.Organisation).Returns(organisation1);
//            A.CallTo(() => memberUpload2.Organisation).Returns(organisation1);

//            // Wire up member uploads to schemes (1-way)
//            A.CallTo(() => memberUpload1.Scheme).Returns(scheme1);
//            A.CallTo(() => memberUpload2.Scheme).Returns(scheme1);

//            // Wire up producers and schemes (2-way).
//            A.CallTo(() => scheme1.Producers).Returns(new List<Producer>
//            {
//                producer1,
//                producer2
//            });

//            // Wire up producers and member uploads (2-way).
//            A.CallTo(() => memberUpload1.Producers).Returns(new List<Producer>
//            {
//                producer1
//            });

//            A.CallTo(() => memberUpload2.Producers).Returns(new List<Producer>
//            {
//                producer2
//            });

//            // Wire up everything to the context (1-way).
//            var weeeContext = A.Fake<WeeeContext>();

//            var schemesDbSet = dbContextHelper.GetAsyncEnabledDbSet(new List<Scheme>
//            {
//                scheme1,
//                scheme2
//            });
//            A.CallTo(() => weeeContext.Schemes).Returns(schemesDbSet);

//            var producersDbSet = dbContextHelper.GetAsyncEnabledDbSet(new List<Producer>
//            {
//                producer1,
//                producer2
//            });
//            A.CallTo(() => weeeContext.Producers).Returns(producersDbSet);

//            var organisationDbSet = dbContextHelper.GetAsyncEnabledDbSet(new List<Organisation>
//            {
//                organisation1,
//                organisation2
//            });
//            A.CallTo(() => weeeContext.Organisations).Returns(organisationDbSet);

//            var memberUploadDbSet = dbContextHelper.GetAsyncEnabledDbSet(new List<MemberUpload>
//            {
//                memberUpload1,
//                memberUpload2
//            });
//            A.CallTo(() => weeeContext.MemberUploads).Returns(memberUploadDbSet);

//            return weeeContext;
//        }
//    }
//}