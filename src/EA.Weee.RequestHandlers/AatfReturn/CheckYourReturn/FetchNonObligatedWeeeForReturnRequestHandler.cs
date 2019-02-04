﻿namespace EA.Weee.RequestHandlers.AatfReturn.CheckYourReturn
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using EA.Prsd.Core.Mediator;
    using EA.Weee.RequestHandlers.DataReturns.FetchDataReturnComplianceYearsForScheme;
    using Security;
    using Request = Requests.AatfReturn.NonObligated.FetchNonObligatedWeeeForReturnRequest;

    public class FetchNonObligatedWeeeForReturnRequestHandler : IRequestHandler<Request, List<decimal?>>
    {
        private readonly IFetchNonObligatedWeeeForReturnDataAccess dataAccess;

        public FetchNonObligatedWeeeForReturnRequestHandler(
            IFetchNonObligatedWeeeForReturnDataAccess dataAccess)
        {
            this.dataAccess = dataAccess;
        }

        public async Task<List<decimal?>> HandleAsync(Request message)
        {
            return await dataAccess.FetchNonObligatedWeeeForReturn(message.ReturnId, message.Dcf);
        }
    }
}
