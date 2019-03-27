﻿namespace EA.Weee.RequestHandlers.AatfReturn.ObligatedSentOn
{
    using EA.Weee.DataAccess;
    using EA.Weee.Domain.AatfReturn;
    using System;
    using System.Data.Entity;
    using System.Linq;
    using System.Threading.Tasks;

    public class SentOnAatfSiteDataAccess : ISentOnAatfSiteDataAccess
    {
        private readonly WeeeContext context;

        public SentOnAatfSiteDataAccess(WeeeContext context)
        {
            this.context = context;
        }

        public Task Submit(WeeeSentOn weeeSentOn)
        {
            context.WeeeSentOn.Add(weeeSentOn);

            return context.SaveChangesAsync();
        }

        public Task UpdateWithOperatorAddress(WeeeSentOn weeeSentOn, AatfAddress @operator)
        {
            weeeSentOn.UpdateWithOperatorAddress(@operator);

            return context.SaveChangesAsync();
        }

        public async Task<AatfAddress> GetWeeeSentOnAddress(Guid id)
        {
            return await context.WeeeSentOn.Where(w => w.Id == id).Select(w => w.SiteAddress).SingleOrDefaultAsync();
        }
    }
}