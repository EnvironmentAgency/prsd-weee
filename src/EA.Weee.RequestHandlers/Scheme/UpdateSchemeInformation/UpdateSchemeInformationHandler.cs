﻿namespace EA.Weee.RequestHandlers.Scheme.UpdateSchemeInformation
{
    using Core.Helpers;
    using Core.Scheme;
    using Domain;
    using Domain.Scheme;
    using EA.Weee.RequestHandlers.Security;
    using Mappings;
    using Prsd.Core.Mediator;
    using Requests.Scheme;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    internal class UpdateSchemeInformationHandler : IRequestHandler<UpdateSchemeInformation, CreateOrUpdateSchemeInformationResult>
    {
        private readonly IWeeeAuthorization authorization;
        private readonly IUpdateSchemeInformationDataAccess dataAccess;

        public UpdateSchemeInformationHandler(
            IWeeeAuthorization authorization,
            IUpdateSchemeInformationDataAccess dataAccess)
        {
            this.authorization = authorization;
            this.dataAccess = dataAccess;
        }

        public async Task<CreateOrUpdateSchemeInformationResult> HandleAsync(UpdateSchemeInformation message)
        {
            authorization.EnsureCanAccessInternalArea();

            Scheme scheme = await dataAccess.FetchSchemeAsync(message.SchemeId);

            /*
             * Check the uniqueness of the approval number if the value is being changed.
             */
            if (scheme.ApprovalNumber != message.ApprovalNumber)
            {
                if (await dataAccess.CheckSchemeApprovalNumberInUseAsync(message.ApprovalNumber))
                {
                    return new CreateOrUpdateSchemeInformationResult()
                    {
                        Result = CreateOrUpdateSchemeInformationResult.ResultType.ApprovalNumberUniquenessFailure
                    };
                }
            }

            UKCompetentAuthority environmentAgency = await dataAccess.FetchEnvironmentAgencyAsync();
            if (environmentAgency.Id == message.CompetentAuthorityId)
            {
                // The 1B1S customer reference is mandatory for schemes in the Environmetn Agency.
                if (string.IsNullOrEmpty(message.IbisCustomerReference))
                {
                    return new CreateOrUpdateSchemeInformationResult()
                    {
                        Result = CreateOrUpdateSchemeInformationResult.ResultType.IbisCustomerReferenceMandatoryForEAFailure,
                    };
                }
                else
                {
                    /*
                     * The 1B1S customer refernece must be unique across schemes within the Environment Agency.
                     *
                     * Try and find another non-rejected scheme for the Environment Agency with the same
                     * 1B1S customer reference. In production, this should at most only ever return one result.
                     * 
                     * As the check for uniqueness has not always existed, it is possible that other
                     * environments may contain multiple schemes with the same 1B1S customer reference
                     * so we are using FirstOrDefault rather than SingleOrDefault.
                     */
                    List<Scheme> nonRejectedEnvironmentAgencySchemes = await dataAccess.FetchNonRejectedEnvironmentAgencySchemesAsync();
                    Scheme otherScheme = nonRejectedEnvironmentAgencySchemes
                        .Where(s => s.Id != scheme.Id)
                        .Where(s => s.IbisCustomerReference == message.IbisCustomerReference)
                        .FirstOrDefault();

                    if (otherScheme != null)
                    {
                        return new CreateOrUpdateSchemeInformationResult()
                        {
                            Result = CreateOrUpdateSchemeInformationResult.ResultType.IbisCustomerReferenceUniquenessFailure,
                            IbisCustomerReferenceUniquenessFailure = new CreateOrUpdateSchemeInformationResult.IbisCustomerReferenceUniquenessFailureInfo()
                            {
                                IbisCustomerReference = message.IbisCustomerReference,
                                OtherSchemeApprovalNumber = otherScheme.ApprovalNumber,
                                OtherSchemeName = otherScheme.SchemeName
                            }
                        };
                    }
                }
            }

            Domain.Obligation.ObligationType obligationType = ValueObjectInitializer.GetObligationType(message.ObligationType);
            scheme.UpdateScheme(
                message.SchemeName,
                message.ApprovalNumber,
                message.IbisCustomerReference,
                obligationType,
                message.CompetentAuthorityId);

            SchemeStatus status = message.Status.ToDomainEnumeration<SchemeStatus>();
            scheme.SetStatus(status);

            await dataAccess.SaveAsync();

            return new CreateOrUpdateSchemeInformationResult()
            {
                Result = CreateOrUpdateSchemeInformationResult.ResultType.Success
            };
        }
    }
}
