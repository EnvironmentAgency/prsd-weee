﻿namespace EA.Weee.RequestHandlers.Factories
{
    using Domain.DataReturns;
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public interface IQuarterWindowFactory
    {
        Task<QuarterWindow> GetQuarterWindow(Quarter quarter);

        Task<List<QuarterWindow>> GetQuarterWindowsForDate(DateTime date);

        Task<QuarterWindow> GetAnnualQuarter(Quarter quarter);

        Task<QuarterWindow> GetNextQuarterWindow(QuarterType q, int year);

        Task<QuarterType> GetAnnualQuarterForDate(DateTime date);
    }
}
