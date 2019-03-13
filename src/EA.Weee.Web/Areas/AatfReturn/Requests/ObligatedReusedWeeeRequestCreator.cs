﻿namespace EA.Weee.Web.Areas.AatfReturn.Requests
{
    using System;
    using System.Collections.Generic;
    using EA.Prsd.Core;
    using ViewModels;
    using Web.Requests.Base;
    using Weee.Requests.AatfReturn.Obligated;

    public class ObligatedReusedWeeeRequestCreator : RequestCreator<ObligatedViewModel, AddObligatedReused>, IObligatedReusedWeeeRequestCreator
    {
        public override AddObligatedReused ViewModelToRequest(ObligatedViewModel viewModel)
        {
            Guard.ArgumentNotNull(() => viewModel, viewModel);
            var obligatedRequestValues = new List<ObligatedValue>();

            foreach (var categoryValue in viewModel.CategoryValues)
            {
                var householdValue = ConvertStringToDecimal(categoryValue.B2C);
                var nonHouseholdValue = ConvertStringToDecimal(categoryValue.B2B);

                obligatedRequestValues.Add(
                    new ObligatedValue(Guid.NewGuid(),
                        categoryValue.CategoryId,
                        householdValue,
                        nonHouseholdValue));
            }

            return new AddObligatedReused() { AatfId = viewModel.AatfId, OrganisationId = viewModel.OrganisationId, ReturnId = viewModel.ReturnId, CategoryValues = obligatedRequestValues };
        }

        private decimal? ConvertStringToDecimal(string input)
        {
            decimal? value = null;
            if (!string.IsNullOrWhiteSpace(input))
            {
                value = Convert.ToDecimal(input);
            }

            return value;
        }
    }
}