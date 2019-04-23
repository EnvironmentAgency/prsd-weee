﻿namespace EA.Weee.Web.Areas.AatfReturn.Requests
{
    using EA.Weee.Requests.AatfReturn.Obligated;
    using EA.Weee.Web.Areas.AatfReturn.ViewModels;

    public class EditSentOnAatfSiteRequestCreator : IEditSentOnAatfSiteRequestCreator
    {
        public EditSentOnAatfSiteWithOperator ViewModelToRequest(SentOnCreateSiteOperatorViewModel viewModel)
        {
            var aatfSite = new EditSentOnAatfSiteWithOperator()
            {
                OrganisationId = viewModel.OrganisationId,
                ReturnId = viewModel.ReturnId,
                AatfId = viewModel.AatfId,
                SiteAddressData = viewModel.SiteAddressData,
                WeeeSentOnId = viewModel.WeeeSentOnId,
                OperatorAddressData = viewModel.OperatorAddressData
            };

            return aatfSite;
        }
    }
}
