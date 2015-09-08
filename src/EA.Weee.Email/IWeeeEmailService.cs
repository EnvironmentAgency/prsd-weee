﻿namespace EA.Weee.Email
{
    using EA.Weee.Domain;
using EA.Weee.Domain.Organisation;
using EA.Weee.Domain.Scheme;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

    public interface IWeeeEmailService
    {
        Task<bool> SendActivateUserAccount(string emailAddress, string activationUrl);

        Task<bool> SendOrganisationUserRequest(string emailAddress, OrganisationUser organisationUser);

        Task<bool> SendOrganisationUserRequestCompleted(OrganisationUser organisationUser);
    }
}
