﻿namespace EA.Weee.Web.ViewModels.Returns
{
    using System;
    using System.Collections.Generic;
    using Core.DataReturns;
    using EA.Weee.Core.Helpers;

    public class ReturnsViewModel
    {
        public Guid OrganisationId { get; set; }

        public string OrganisationName { get; set; }

        public IList<ReturnsItemViewModel> Returns { get; set; }

        public int ComplianceYear { get; set; }

        public QuarterType Quarter { get; set; }

        public bool DisplayCreateReturn { get; set; }

        public ReturnsViewModel()
        {
            Returns = new List<ReturnsItemViewModel>();
        }

        public string GetErrorMessageForNotAllowingCreateReturn()
        {
            if (WindowHelper.IsWindowClosed())
            {
                return "Window closed";
            }

            return "";
        }
    }
}