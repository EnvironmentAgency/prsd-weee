﻿namespace EA.Weee.RequestHandlers.Search.FetchOrganisationSearchResultsForCache
{
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Linq;
    using System.Threading.Tasks;
    using EA.Prsd.Core.Mapper;
    using EA.Weee.Core.Search;
    using EA.Weee.Core.Shared;
    using EA.Weee.DataAccess;
    using EA.Weee.Domain.AatfReturn;
    using EA.Weee.Domain.Organisation;
    using EA.Weee.Domain.Scheme;

    public class FetchOrganisationSearchResultsForCacheDataAccess : IFetchOrganisationSearchResultsForCacheDataAccess
    {
        private readonly WeeeContext context;
        private readonly IMap<Address, AddressData> addressMapper;

        public FetchOrganisationSearchResultsForCacheDataAccess(WeeeContext context, IMap<Address, AddressData> addressMapper)
        {
            this.context = context;
            this.addressMapper = addressMapper;
        }

        /// <summary>
        /// Returns a list of all complete organisations, ordered by organisation name.
        /// For now, only organisations representing schemes will be returned, excluding
        /// any scheme that has a status of rejected.
        /// </summary>
        /// <returns></returns>
        public async Task<IList<OrganisationSearchResult>> FetchCompleteOrganisations()
        {
            var organisations = await context.Organisations
                .Where(p => p.OrganisationStatus.Value == Domain.Organisation.OrganisationStatus.Complete.Value)
                .ToListAsync();

            var schemes = await context.Schemes.ToListAsync();

            foreach (Scheme scheme in schemes)
            {
                if (scheme.SchemeStatus.Value == Domain.Scheme.SchemeStatus.Rejected.Value)
                {
                    organisations.Remove(scheme.Organisation);
                }
            }

            var aatfs = await context.Aatfs.ToListAsync();

            return organisations.Select(r => new OrganisationSearchResult()
            {
                OrganisationId = r.Id,
                Name = r.OrganisationName,
                Address = addressMapper.Map(r.BusinessAddress),
                PcsCount = schemes.Count(p => p.OrganisationId == r.Id),
                AatfCount = aatfs.Count(p => p.Organisation.Id == r.Id && p.FacilityType == FacilityType.Aatf),
                AeCount = aatfs.Count(p => p.Organisation.Id == r.Id && p.FacilityType == FacilityType.Ae)
            })
                .OrderBy(r => r.Name)
                .ToList();
        }
    }
}
