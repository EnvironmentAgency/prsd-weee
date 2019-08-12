﻿namespace EA.Weee.RequestHandlers.Admin.DeleteAatf.DeleteValidation
{
    using System;
    using System.Threading.Tasks;
    using AatfReturn.Internal;
    using Core.Admin;
    using DataAccess.DataAccess;

    public class GetAatfDeletionStatus : IGetAatfDeletionStatus
    {
        private readonly IAatfDataAccess aatfDataAccess;
        private readonly IGetOrganisationDeletionStatus getOrganisationDeletionStatus;

        public GetAatfDeletionStatus(IAatfDataAccess aatfDataAccess, 
            IGetOrganisationDeletionStatus getOrganisationDeletionStatus)
        {
            this.aatfDataAccess = aatfDataAccess;
            this.getOrganisationDeletionStatus = getOrganisationDeletionStatus;
        }

        public async Task<CanAatfBeDeletedFlags> Validate(Guid aatfId)
        {
            var result = new CanAatfBeDeletedFlags();

            var aatf = await aatfDataAccess.GetDetails(aatfId);

            var hasData = await aatfDataAccess.HasAatfData(aatfId);

            if (hasData)
            {
                result |= CanAatfBeDeletedFlags.HasData;
                return result;
            }

            var organisationDeletionStatus = await getOrganisationDeletionStatus.Validate(aatf.Organisation.Id, aatf.ComplianceYear);

            if (organisationDeletionStatus.HasFlag(CanOrganisationBeDeletedFlags.HasReturns) &&
                await aatfDataAccess.HasAatfOrganisationOtherEntities(aatfId))
            {
                result |= CanAatfBeDeletedFlags.CanDelete;
            }
            else if (!organisationDeletionStatus.HasFlag(CanOrganisationBeDeletedFlags.HasReturns) &&
                     await aatfDataAccess.HasAatfOrganisationOtherEntities(aatfId))
            {
                result |= CanAatfBeDeletedFlags.CanDelete;
            }
            else if (!organisationDeletionStatus.HasFlag(CanOrganisationBeDeletedFlags.HasReturns) &&
                     !await aatfDataAccess.HasAatfOrganisationOtherEntities(aatfId))
            {
                result |= CanAatfBeDeletedFlags.CanDelete;
                result |= CanAatfBeDeletedFlags.CanDeleteOrganisation;
            }

            return result;
        }
    }
}