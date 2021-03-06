﻿namespace EA.Weee.Web.Tests.Unit.Areas.AatfReturn.ViewModels
{
    using Core.DataReturns;
    using EA.Weee.Core.Helpers;
    using FluentAssertions;
    using System;
    using System.Globalization;
    using System.Linq;
    using Web.Areas.AatfReturn.ViewModels;
    using Xunit;

    public class ObligatedReusedViewModelTests
    {
        private readonly ObligatedViewModel viewModel;
        private readonly ICategoryValueTotalCalculator calculator;

        public ObligatedReusedViewModelTests()
        {
            calculator = new CategoryValueTotalCalculator();
            viewModel = new ObligatedViewModel(calculator);
        }

        [Fact]
        public void GivenObligatedReusedViewModel_CategoriesShouldBePopulated()
        {
            viewModel.CategoryValues.Should().NotBeNull();
            viewModel.CategoryValues.Count().Should().Be(Enum.GetNames(typeof(WeeeCategory)).Length);
        }

        [Fact]
        public void Totals_GivenObligatedReusedViewModel_TotalsShouldBeZero()
        {
            viewModel.B2CTotal.Should().Be("0.000");
            viewModel.B2BTotal.Should().Be("0.000");
        }

        [Fact]
        public void Totals_GivenObligatedReusedViewModelWithValues_TotalsShouldBeCorrect()
        {
            for (var count = 0; count < viewModel.CategoryValues.Count; count++)
            {
                viewModel.CategoryValues.ElementAt(count).B2C = (count + 1).ToString();
                viewModel.CategoryValues.ElementAt(count).B2B = (count + 1).ToString();
            }

            viewModel.B2BTotal.Should().Be("105.000");
            viewModel.B2CTotal.Should().Be("105.000");
        }

        [Fact]
        public void Totals_GivenObligatedReusedViewModelWithDecimalValues_TotalsShouldBeCorrect()
        {
            for (var count = 0; count < viewModel.CategoryValues.Count; count++)
            {
                viewModel.CategoryValues.ElementAt(count).B2C = (count * 0.001m).ToString(CultureInfo.InvariantCulture);
                viewModel.CategoryValues.ElementAt(count).B2B = (count * 0.001m).ToString(CultureInfo.InvariantCulture);
            }

            viewModel.B2CTotal.Should().Be("0.091");
            viewModel.B2BTotal.Should().Be("0.091");
        }

        [Fact]
        public void Totals_GivenObligatedReusedViewModelWithNullValues_TotalsShouldBeCorrect()
        {
            viewModel.CategoryValues.ElementAt(2).B2C = 1.ToString();
            viewModel.CategoryValues.ElementAt(4).B2C = 2.ToString();
            viewModel.CategoryValues.ElementAt(3).B2B = 3.ToString();
            viewModel.CategoryValues.ElementAt(5).B2B = 4.ToString();

            viewModel.B2CTotal.Should().Be("3.000");
            viewModel.B2BTotal.Should().Be("7.000");
        }

        [Theory]
        [InlineData(null)]
        [InlineData("AAA")]
        [InlineData(" ")]
        [InlineData("")]
        public void Totals_GivenObligatedReusedViewModelWithInvalidValues_TotalsShouldBeCorrect(string value)
        {
            viewModel.CategoryValues.ElementAt(2).B2C = value;
            viewModel.CategoryValues.ElementAt(4).B2C = value;
            viewModel.CategoryValues.ElementAt(3).B2B = value;
            viewModel.CategoryValues.ElementAt(5).B2B = value;

            viewModel.B2CTotal.Should().Be("0.000");
            viewModel.B2BTotal.Should().Be("0.000");
        }
    }
}
