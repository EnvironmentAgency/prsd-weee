﻿namespace EA.Weee.Web.Areas.AatfReturn.Requests
{
    using EA.Weee.Core.AatfReturn;
    using EA.Weee.Requests.AatfReturn;
    using EA.Weee.Web.Areas.AatfReturn.ViewModels;
    using System;
    using System.Collections.Generic;

    public class DeleteReturnDataRequestCreator : IDeleteReturnDataRequestCreator
    {
        public AddReturnReportOn ViewModelToRequest(SelectReportOptionsNilViewModel viewModel)
        {
            var reportOptions = new AddReturnReportOn()
            {
                ReturnId = viewModel.ReturnId,
                DeselectedOptions = new List<int>()
            };

            foreach (var question in Enum.GetValues(typeof(ReportOnQuestionEnum)))
            {
                reportOptions.DeselectedOptions.Add((int)question);
            }

            return reportOptions;
        }
    }
}
