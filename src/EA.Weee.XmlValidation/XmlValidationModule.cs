﻿namespace EA.Weee.XmlValidation
{
    using Autofac;
    using BusinessValidation;
    using BusinessValidation.Helpers;
    using BusinessValidation.QuerySets;
    using BusinessValidation.QuerySets.Queries.Producer;
    using BusinessValidation.Rules.Producer;
    using BusinessValidation.Rules.Scheme;
    using Errors;
    using SchemaValidation;

    public class XmlValidationModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            // TODO: Autoscan

            builder.RegisterType<XmlBusinessValidator>()
                .As<IXmlBusinessValidator>()
                .InstancePerRequest();

            builder.RegisterType<MigratedProducerQuerySet>()
                .As<IMigratedProducerQuerySet>()
                .InstancePerRequest();

            builder.RegisterType<ProducerQuerySet>()
                .As<IProducerQuerySet>()
                .InstancePerRequest();

            builder.RegisterType<SchemeQuerySet>()
                .As<ISchemeQuerySet>()
                .InstancePerRequest();

            builder.RegisterType<AmendmentHasNoProducerRegistrationNumber>()
                .As<IAmendmentHasNoProducerRegistrationNumber>()
                .InstancePerRequest();

            builder.RegisterType<AnnualTurnoverMismatch>()
                .As<IAnnualTurnoverMismatch>()
                .InstancePerRequest();

            builder.RegisterType<InsertHasProducerRegistrationNumber>()
                .As<IInsertHasProducerRegistrationNumber>()
                .InstancePerRequest();

            builder.RegisterType<ProducerAlreadyRegistered>()
                .As<IProducerAlreadyRegistered>()
                .InstancePerRequest();

            builder.RegisterType<ProducerNameAlreadyRegistered>()
                .As<IProducerNameAlreadyRegistered>()
                .InstancePerRequest();

            builder.RegisterType<ProducerNameChange>()
                .As<IProducerNameChange>()
                .InstancePerRequest();

            builder.RegisterType<ProducerRegistrationNumberValidity>()
                .As<IProducerRegistrationNumberValidity>()
                .InstancePerRequest();

            builder.RegisterType<UkBasedAuthorisedRepresentative>()
                .As<IUkBasedAuthorisedRepresentative>()
                .InstancePerRequest();

            builder.RegisterType<CorrectSchemeApprovalNumber>()
                .As<ICorrectSchemeApprovalNumber>()
                .InstancePerRequest();

            builder.RegisterType<DuplicateProducerNames>()
                .As<IDuplicateProducerNames>()
                .InstancePerRequest();

            builder.RegisterType<DuplicateProducerRegistrationNumbers>()
                .As<IDuplicateProducerRegistrationNumbers>()
                .InstancePerRequest();

            builder.RegisterType<SearchMatcher>()
                .As<ISearchMatcher>()
                .InstancePerRequest();

            builder.RegisterType<EnsureAnOverseasProducerIsNotBasedInTheUK>()
                .As<IEnsureAnOverseasProducerIsNotBasedInTheUK>()
                .InstancePerRequest();

            builder.RegisterType<ProducerChargeBandChange>()
                .As<IProducerChargeBandChange>()
                .InstancePerRequest();

            builder.RegisterType<CompanyAlreadyRegistered>()
                .As<ICompanyAlreadyRegistered>()
                .InstancePerRequest();

            builder.RegisterType<ExistingProducerNames>().As<IExistingProducerNames>().InstancePerRequest();
            builder.RegisterType<CurrentCompanyProducers>().As<ICurrentCompanyProducers>().InstancePerRequest();
            builder.RegisterType<ExistingProducerRegistrationNumbers>().As<IExistingProducerRegistrationNumbers>().InstancePerRequest();
            builder.RegisterType<CurrentProducersByRegistrationNumber>().As<ICurrentProducersByRegistrationNumber>().InstancePerRequest();

            builder.RegisterType<XmlErrorTranslator>().As<IXmlErrorTranslator>().InstancePerRequest();
            builder.RegisterType<SchemaValidator>().As<ISchemaValidator>().InstancePerRequest();
        }
    }
}
