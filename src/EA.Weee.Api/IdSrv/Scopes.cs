﻿namespace EA.Weee.Api.IdSrv
{
    using EA.Weee.Core;
    using IdentityServer3.Core.Models;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Security.Claims;

    internal static class Scopes
    {
        public static List<Scope> Get()
        {
            // This list of scopes provided by the identity server
            // must match those requested by EA.Prsd.Core.Web.OAuth.OAuthClient

            var scopes = new List<Scope>
            {
                new Scope
                {
                    Name = "api1",
                    DisplayName = "WEEE API",
                    Type = ScopeType.Resource,
                    Emphasize = true,
                    Claims = new List<ScopeClaim>()
                    {
                        // This list defines which claims will be returned by calls
                        // to /connect/userinfo which are used to generate the ClaimsPrincipal
                        // used by the MVC front end and the API.

                        new ScopeClaim(ClaimTypes.AuthenticationMethod),
                        new ScopeClaim(WeeeClaimTypes.OrganisationAccess),
                        new ScopeClaim(WeeeClaimTypes.SchemeAccess)
                    }
                },
                new Scope()
                {
                    Name = "api2",
                    DisplayName = "WEEE API Unauthenticated",
                    Type = ScopeType.Resource,
                    Emphasize = true
                },
                StandardScopes.OpenId,
                StandardScopes.AllClaims,
                StandardScopes.Profile,
                StandardScopes.OfflineAccess
            };

            return scopes;
        }
    }
}