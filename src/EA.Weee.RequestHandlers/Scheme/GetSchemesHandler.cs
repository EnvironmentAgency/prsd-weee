﻿namespace EA.Weee.RequestHandlers.Scheme
{
    using Core.Scheme;
    using DataAccess;
    using Domain.Organisation;
    using Domain.Scheme;
    using EA.Weee.RequestHandlers.Security;
    using Prsd.Core.Mapper;
    using Prsd.Core.Mediator;
    using Requests.Scheme;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Linq;
    using System.Threading.Tasks;

    internal class GetSchemesHandler : IRequestHandler<GetSchemes, List<SchemeData>>
    {
        private readonly WeeeContext context;
        private readonly IMap<Scheme, SchemeData> schemeMap;
        private readonly IWeeeAuthorization authorization;

        public GetSchemesHandler(WeeeContext context, IMap<Scheme, SchemeData> schemeMap, IWeeeAuthorization authorization)
        {
            this.context = context;
            this.schemeMap = schemeMap;
            this.authorization = authorization;
        }

        public async Task<List<SchemeData>> HandleAsync(GetSchemes message)
        {
            authorization.EnsureCanAccessInternalArea();

            var schemes = await context.Schemes
                .Where(s => s.Organisation.OrganisationStatus.Value == OrganisationStatus.Complete.Value)
                .ToListAsync();
             
            return schemes.Select(s => schemeMap.Map(s))
                .OrderBy(sd => sd.Name)
                .ToList();
        }
    }
}
