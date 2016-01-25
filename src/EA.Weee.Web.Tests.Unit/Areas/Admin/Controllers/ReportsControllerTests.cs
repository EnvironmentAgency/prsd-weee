﻿namespace EA.Weee.Web.Tests.Unit.Areas.Admin.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using System.Web.Mvc;
    using Api.Client;
    using Core.Admin;
    using Core.Scheme;
    using Core.Shared;
    using FakeItEasy;
    using Prsd.Core;
    using Prsd.Core.Mediator;
    using Services;
    using Web.Areas.Admin.Controllers;
    using Web.Areas.Admin.ViewModels.Reports;
    using Weee.Requests.Admin;
    using Weee.Requests.Admin.GetActiveComplianceYears;
    using Weee.Requests.Admin.Reports;
    using Weee.Requests.Scheme;
    using Weee.Requests.Shared;
    using Xunit;

    public class ReportsControllerTests
    {
        /// <summary>
        /// These tests ensure that the GET "Index" action prevents users who are inactive, pending or rejected
        /// from accessing the index action by redirecting them to the "InternalUserAuthorizationRequired"
        /// action of the account controller.
        /// </summary>
        /// <param name="userStatus"></param>
        /// <returns></returns>
        [Theory]
        [InlineData(UserStatus.Inactive)]
        [InlineData(UserStatus.Pending)]
        [InlineData(UserStatus.Rejected)]
        public async Task GetIndex_WhenUserIsNotActive_RedirectsToInternalUserAuthorizationRequired(UserStatus userStatus)
        {
            // Arrange
            IWeeeClient client = A.Fake<IWeeeClient>();
            A.CallTo(() => client.SendAsync(A<string>._, A<IRequest<UserStatus>>._)).Returns(userStatus);

            ReportsController controller = new ReportsController(
                () => client,
                A.Dummy<BreadcrumbService>());

            // Act
            ActionResult result = await controller.Index();

            // Assert
            RedirectToRouteResult redirectResult = result as RedirectToRouteResult;
            Assert.NotNull(redirectResult);

            Assert.Equal("InternalUserAuthorisationRequired", redirectResult.RouteValues["action"]);
            Assert.Equal("Account", redirectResult.RouteValues["controller"]);
            Assert.Equal(userStatus, redirectResult.RouteValues["userStatus"]);
        }

        /// <summary>
        /// This test ensures that the GET "Index" action will redirect an active user to the "ChooseReport" action.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task GetIndex_WhenUserIsActive_RedirectsToChooseReport()
        {
            // Arrange
            IWeeeClient client = A.Fake<IWeeeClient>();
            A.CallTo(() => client.SendAsync(A<string>._, A<IRequest<UserStatus>>._)).Returns(UserStatus.Active);

            ReportsController controller = new ReportsController(
                () => client,
                A.Dummy<BreadcrumbService>());

            // Act
            ActionResult result = await controller.Index();

            // Assert
            RedirectToRouteResult redirectResult = result as RedirectToRouteResult;
            Assert.NotNull(redirectResult);

            Assert.Equal("ChooseReport", redirectResult.RouteValues["action"]);
            Assert.Equal("Reports", redirectResult.RouteValues["controller"]);
        }

        /// <summary>
        /// This test ensures that the GET "ChooseReport" action returns the "ChooseReport" view with
        /// a populated view model.
        /// </summary>
        [Fact]
        public void GetChooseReport_Always_ReturnsChooseReportView()
        {
            // Arrange
            ReportsController controller = new ReportsController(
                () => A.Dummy<IWeeeClient>(),
                A.Dummy<BreadcrumbService>());

            // Act
            ActionResult result = controller.ChooseReport();

            // Assert
            ViewResult viewResult = result as ViewResult;
            Assert.NotNull(viewResult);

            Assert.True(string.IsNullOrEmpty(viewResult.ViewName) || viewResult.ViewName.ToLowerInvariant() == "choosereport");

            ChooseReportViewModel viewModel = viewResult.Model as ChooseReportViewModel;
            Assert.NotNull(viewModel);
        }

        /// <summary>
        /// This test ensures that the POST "ChooseReport" action will return the "ChooseReport"
        /// view when an invalid model is provided.
        /// </summary>
        [Fact]
        public void PostChooseReport_ModelIsInvalid_ReturnsChooseReportView()
        {
            // Arrange
            ReportsController controller = new ReportsController(
                () => A.Dummy<IWeeeClient>(),
                A.Dummy<BreadcrumbService>());

            controller.ModelState.AddModelError("Key", "Any error");

            // Act
            ActionResult result = controller.ChooseReport(A.Dummy<ChooseReportViewModel>());

            // Assert
            ViewResult viewResult = result as ViewResult;
            Assert.NotNull(viewResult);

            Assert.True(string.IsNullOrEmpty(viewResult.ViewName) || viewResult.ViewName.ToLowerInvariant() == "choosereport");
        }

        /// <summary>
        /// This test ensures that the POST "ChooseReport" action will redirect to the
        /// action represented by each of the possible valid options.
        /// </summary>
        /// <param name="selectedValue"></param>
        /// <param name="expectedAction"></param>
        [Theory]
        [InlineData(Reports.ProducerDetails, "ProducerDetails")]
        [InlineData(Reports.ProducerPublicRegister, "ProducerPublicRegister")]
        [InlineData(Reports.UKWeeeData, "UKWeeeData")]
        [InlineData(Reports.ProducerEeeData, "ProducerEeeData")]
        [InlineData(Reports.SchemeWeeeData, "SchemeWeeeData")]
        [InlineData(Reports.UKEEEData, "UKEEEData")]
        public void PostChooseReport_WithSelectedValue_RedirectsToExpectedAction(string selectedValue, string expectedAction)
        {
            // Arrange
            ReportsController controller = new ReportsController(
               () => A.Dummy<IWeeeClient>(),
               A.Dummy<BreadcrumbService>());

            // Act
            ChooseReportViewModel model = new ChooseReportViewModel { SelectedValue = selectedValue };
            ActionResult result = controller.ChooseReport(model);

            // Assert
            RedirectToRouteResult redirectResult = result as RedirectToRouteResult;
            Assert.NotNull(redirectResult);

            Assert.Equal(expectedAction, redirectResult.RouteValues["action"]);
        }

        /// <summary>
        /// This test ensures that the POST "ChooseReport" action will throw a
        /// NotSupportedException when the selected value is not one of the
        /// allowed values.
        /// </summary>
        [Fact]
        public void PostChooseReport_WithInvalidSelectedValue_ThrowsNotSupportedException()
        {
            // Arrange
            ReportsController controller = new ReportsController(
               () => A.Dummy<IWeeeClient>(),
               A.Dummy<BreadcrumbService>());

            // Act
            ChooseReportViewModel model = new ChooseReportViewModel { SelectedValue = "SOME INVALID VALUE" };
            Func<ActionResult> testCode = () => controller.ChooseReport(model);

            // Assert
            Assert.Throws<NotSupportedException>(testCode);
        }

        /// <summary>
        /// This test ensures that the GET "ProducerDetails" action calls the API to populate
        /// the report filters and returns the "ProducerDetails" view.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task GetProducerDetails_Always_PopulatesFiltersAndReturnsProducerDetailsView()
        {
            // Arrange
            IWeeeClient weeeClient = A.Fake<IWeeeClient>();

            List<int> years = new List<int>() { 2001, 2002 };
            A.CallTo(() => weeeClient.SendAsync(A<string>._, A<GetMemberRegistrationsActiveComplianceYears>._)).Returns(years);

            UKCompetentAuthorityData authority1 = new UKCompetentAuthorityData()
            {
                Id = new Guid("EB44DAD0-6B47-47C1-9124-4BE91042E563"),
                Abbreviation = "AA1"
            };
            List<UKCompetentAuthorityData> authorities = new List<UKCompetentAuthorityData>() { authority1 };
            A.CallTo(() => weeeClient.SendAsync(A<string>._, A<GetUKCompetentAuthorities>._)).Returns(authorities);

            SchemeData scheme1 = new SchemeData()
            {
                Id = new Guid("0F638399-226F-4942-AEF1-E6BC7EB447D6"),
                SchemeName = "Test Scheme"
            };
            List<SchemeData> schemes = new List<SchemeData>() { scheme1 };
            A.CallTo(() => weeeClient.SendAsync(A<string>._, A<GetAllApprovedSchemes>._)).Returns(schemes);

            ReportsController controller = new ReportsController(
                () => weeeClient,
                A.Dummy<BreadcrumbService>());

            // Act
            ActionResult result = await controller.ProducerDetails();

            // Assert
            ViewResult viewResult = result as ViewResult;
            Assert.NotNull(viewResult);
            Assert.True(string.IsNullOrEmpty(viewResult.ViewName) || viewResult.ViewName.ToLowerInvariant() == "producerdetails");

            ReportsFilterViewModel model = viewResult.Model as ReportsFilterViewModel;
            Assert.NotNull(model);

            Assert.Collection(model.ComplianceYears,
                y1 => Assert.Equal("2001", y1.Text),
                y2 => Assert.Equal("2002", y2.Text));

            Assert.Collection(model.AppropriateAuthorities,
                a1 => { Assert.Equal("EB44DAD0-6B47-47C1-9124-4BE91042E563", a1.Value, true); Assert.Equal("AA1", a1.Text); });

            Assert.Collection(model.SchemeNames,
                s1 => { Assert.Equal("0F638399-226F-4942-AEF1-E6BC7EB447D6", s1.Value, true); Assert.Equal("Test Scheme", s1.Text); });
        }

        /// <summary>
        /// This test ensures that the GET "ProducerDetails" action returns
        /// a view with the ViewBag property "TriggerDownload" set to false.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task GetProducerDetails_Always_SetsTriggerDownloadToFalse()
        {
            // Arrange
            ReportsController controller = new ReportsController(
                () => A.Dummy<IWeeeClient>(),
                A.Dummy<BreadcrumbService>());

            // Act
            ActionResult result = await controller.ProducerDetails();

            // Assert
            ViewResult viewResult = result as ViewResult;
            Assert.NotNull(viewResult);
            Assert.Equal(false, viewResult.ViewBag.TriggerDownload);
        }

        /// <summary>
        /// This test ensures that the GET "ProducerDetails" action sets
        /// the breadcrumb's internal activity to "View reports".
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task GetProducerDetails_Always_SetsInternalBreadcrumbToViewReports()
        {
            BreadcrumbService breadcrumb = new BreadcrumbService();

            // Arrange
            ReportsController controller = new ReportsController(
                () => A.Dummy<IWeeeClient>(),
                breadcrumb);

            // Act
            ActionResult result = await controller.ProducerDetails();

            // Assert
            Assert.Equal("View reports", breadcrumb.InternalActivity);
        }

        /// <summary>
        /// This test ensures that the POST "ProducerDetails" action with an invalid view model
        /// calls the API to populate the report filters and returns the "ProducerDetails" view.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task PostProducerDetails_WithInvalidViewModel_ReturnsSchemeWeeeDataProducerDataViewModel()
        {
            // Arrange
            IWeeeClient weeeClient = A.Fake<IWeeeClient>();

            List<int> years = new List<int>() { 2001, 2002 };
            A.CallTo(() => weeeClient.SendAsync(A<string>._, A<GetMemberRegistrationsActiveComplianceYears>._)).Returns(years);

            UKCompetentAuthorityData authority1 = new UKCompetentAuthorityData()
            {
                Id = new Guid("EB44DAD0-6B47-47C1-9124-4BE91042E563"),
                Abbreviation = "AA1"
            };
            List<UKCompetentAuthorityData> authorities = new List<UKCompetentAuthorityData>() { authority1 };
            A.CallTo(() => weeeClient.SendAsync(A<string>._, A<GetUKCompetentAuthorities>._)).Returns(authorities);

            SchemeData scheme1 = new SchemeData()
            {
                Id = new Guid("0F638399-226F-4942-AEF1-E6BC7EB447D6"),
                SchemeName = "Test Scheme"
            };
            List<SchemeData> schemes = new List<SchemeData>() { scheme1 };
            A.CallTo(() => weeeClient.SendAsync(A<string>._, A<GetAllApprovedSchemes>._)).Returns(schemes);

            ReportsController controller = new ReportsController(
                () => weeeClient,
                A.Dummy<BreadcrumbService>());

            // Act
            controller.ModelState.AddModelError("Key", "Error");
            ActionResult result = await controller.ProducerDetails(A.Dummy<ReportsFilterViewModel>());

            // Assert
            ViewResult viewResult = result as ViewResult;
            Assert.NotNull(viewResult);
            Assert.True(string.IsNullOrEmpty(viewResult.ViewName) || viewResult.ViewName.ToLowerInvariant() == "producerdetails");

            ReportsFilterViewModel model = viewResult.Model as ReportsFilterViewModel;
            Assert.NotNull(model);

            Assert.Collection(model.ComplianceYears,
                y1 => Assert.Equal("2001", y1.Text),
                y2 => Assert.Equal("2002", y2.Text));

            Assert.Collection(model.AppropriateAuthorities,
                a1 => { Assert.Equal("EB44DAD0-6B47-47C1-9124-4BE91042E563", a1.Value, true); Assert.Equal("AA1", a1.Text); });

            Assert.Collection(model.SchemeNames,
                s1 => { Assert.Equal("0F638399-226F-4942-AEF1-E6BC7EB447D6", s1.Value, true); Assert.Equal("Test Scheme", s1.Text); });
        }

        /// <summary>
        /// This test ensures that the POST "ProducerDetails" action with an invalid view model
        /// returns a view with the ViewBag property "TriggerDownload" set to false.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task PostProducerDetails_WithInvalidViewModel_SetsTriggerDownloadToFalse()
        {
            // Arrange
            ReportsController controller = new ReportsController(
                () => A.Dummy<IWeeeClient>(),
                A.Dummy<BreadcrumbService>());

            ProducersDataViewModel viewModel = new ProducersDataViewModel();

            // Act
            controller.ModelState.AddModelError("Key", "Error");
            ActionResult result = await controller.ProducerDetails(A.Dummy<ReportsFilterViewModel>());

            // Assert
            ViewResult viewResult = result as ViewResult;
            Assert.NotNull(viewResult);
            Assert.Equal(false, viewResult.ViewBag.TriggerDownload);
        }

        /// <summary>
        /// This test ensures that the POST "ProducerDetails" action with a valid view model
        /// returns a view with the ViewBag property "TriggerDownload" set to true.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task PostProducerDetails_WithViewModel_SetsTriggerDownloadToTrue()
        {
            // Arrange
            ReportsController controller = new ReportsController(
                () => A.Dummy<IWeeeClient>(),
                A.Dummy<BreadcrumbService>());

            // Act
            ActionResult result = await controller.ProducerDetails(A.Dummy<ReportsFilterViewModel>());

            // Assert
            ViewResult viewResult = result as ViewResult;
            Assert.NotNull(viewResult);
            Assert.Equal(true, viewResult.ViewBag.TriggerDownload);
        }

        /// <summary>
        /// This test ensures that the POST "ProducerDetails" action sets
        /// the breadcrumb's internal activity to "View reports".
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task PostProducerDetails_Always_SetsInternalBreadcrumbToViewReports()
        {
            BreadcrumbService breadcrumb = new BreadcrumbService();

            // Arrange
            ReportsController controller = new ReportsController(
                () => A.Dummy<IWeeeClient>(),
                breadcrumb);

            // Act
            ActionResult result = await controller.ProducerDetails(A.Dummy<ReportsFilterViewModel>());

            // Assert
            Assert.Equal("View reports", breadcrumb.InternalActivity);
        }

        /// <summary>
        /// This test ensures that the GET "DownloadProducerDetailsCsv" action when called with no scheme ID and no
        /// authority ID will return a file with a name in the format "2015_producerdetails_31122016_2359" containing
        /// the specified compliance year and the current time.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task GetDownloadProducerDetailsCsv_WithNoSchemeIdAndNoAuthorityId_ReturnsFileNameWithComplianceYearAndCurrentTime()
        {
            // Arrange
            IWeeeClient client = A.Fake<IWeeeClient>();

            CSVFileData file = new CSVFileData() { FileContent = "Content" };
            A.CallTo(() => client.SendAsync(A<string>._, A<GetMemberDetailsCSV>._))
                .Returns(file);

            ReportsController controller = new ReportsController(
                () => client,
                A.Dummy<BreadcrumbService>());

            // Act
            SystemTime.Freeze(new DateTime(2016, 12, 31, 23, 59, 58));
            ActionResult result = await controller.DownloadProducerDetailsCsv(2015, null, null, false);
            SystemTime.Unfreeze();

            // Assert
            FileResult fileResult = result as FileResult;
            Assert.NotNull(fileResult);

            Assert.Equal("2015_producerdetails_31122016_2359.csv", fileResult.FileDownloadName);
        }

        /// <summary>
        /// This test ensures that the GET "DownloadProducerDetailsCsv" action when called with a scheme ID and no
        /// authority ID will return a file with a name in the format "2015_WEEAA1111AASCH_producerdetails_31122016_2359"
        /// containing the specified compliance year, the specified scheme's approval number and the current time.
        /// The forward slashes in the scheme approval number must be removed.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task GetDownloadProducerDetailsCsv_WithSchemeIdAndNoAuthorityId_ReturnsFileNameWithComplianceYearSchemeApprovalNumberAndCurrentTime()
        {
            // Arrange
            IWeeeClient client = A.Fake<IWeeeClient>();

            CSVFileData file = new CSVFileData() { FileContent = "Content" };
            A.CallTo(() => client.SendAsync(A<string>._, A<GetMemberDetailsCSV>._))
                .Returns(file);

            SchemeData schemeData = new SchemeData() { ApprovalName = "WEE/AA1111AA/SCH" };
            A.CallTo(() => client.SendAsync(A<string>._, A<GetSchemeById>._))
                .WhenArgumentsMatch(a => a.Get<GetSchemeById>("request").SchemeId == new Guid("88DF333E-6B2B-4D72-B411-7D7024EAA5F5"))
                .Returns(schemeData);

            ReportsController controller = new ReportsController(
                () => client,
                A.Dummy<BreadcrumbService>());

            // Act
            SystemTime.Freeze(new DateTime(2016, 12, 31, 23, 59, 58));
            ActionResult result = await controller.DownloadProducerDetailsCsv(2015, new Guid("88DF333E-6B2B-4D72-B411-7D7024EAA5F5"), null, false);
            SystemTime.Unfreeze();

            // Assert
            FileResult fileResult = result as FileResult;
            Assert.NotNull(fileResult);

            Assert.Equal("2015_WEEAA1111AASCH_producerdetails_31122016_2359.csv", fileResult.FileDownloadName);
        }

        /// <summary>
        /// This test ensures that the GET "DownloadProducerDetailsCsv" action when called with a scheme ID and an
        /// authority ID will return a file with a name in the format "2015_WEEAA1111AASCH_AA_producerdetails_31122016_2359"
        /// containing the specified compliance year, the specified scheme's approval number, the authority abbreviation and the current time.
        /// The forward slashes in the scheme approval number must be removed.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task GetDownloadProducerDetailsCsv_WithSchemeIdAndAuthorityId_ReturnsFileNameWithComplianceYearSchemeApprovalNumberAuthorityAbbreviationAndCurrentTime()
        {
            // Arrange
            IWeeeClient client = A.Fake<IWeeeClient>();

            CSVFileData file = new CSVFileData() { FileContent = "Content" };
            A.CallTo(() => client.SendAsync(A<string>._, A<GetMemberDetailsCSV>._))
                .Returns(file);

            SchemeData schemeData = new SchemeData() { ApprovalName = "WEE/AA1111AA/SCH" };
            A.CallTo(() => client.SendAsync(A<string>._, A<GetSchemeById>._))
                .WhenArgumentsMatch(a => a.Get<GetSchemeById>("request").SchemeId == new Guid("88DF333E-6B2B-4D72-B411-7D7024EAA5F5"))
                .Returns(schemeData);

            UKCompetentAuthorityData authorityData = new UKCompetentAuthorityData() { Abbreviation = "AA" };
            A.CallTo(() => client.SendAsync(A<string>._, A<GetUKCompetentAuthorityById>._))
                .WhenArgumentsMatch(a => a.Get<GetUKCompetentAuthorityById>("request").Id == new Guid("703839B3-A081-4491-92B7-FCF969067EA3"))
                .Returns(authorityData);

            ReportsController controller = new ReportsController(
                () => client,
                A.Dummy<BreadcrumbService>());

            // Act
            SystemTime.Freeze(new DateTime(2016, 12, 31, 23, 59, 58));
            ActionResult result = await controller.DownloadProducerDetailsCsv(
                2015,
                new Guid("88DF333E-6B2B-4D72-B411-7D7024EAA5F5"),
                new Guid("703839B3-A081-4491-92B7-FCF969067EA3"),
                false);
            SystemTime.Unfreeze();

            // Assert
            FileResult fileResult = result as FileResult;
            Assert.NotNull(fileResult);

            Assert.Equal("2015_WEEAA1111AASCH_AA_producerdetails_31122016_2359.csv", fileResult.FileDownloadName);
        }

        /// <summary>
        /// This test ensures that the GET "DownloadProducerDetailsCsv" action when called without a scheme ID and with an
        /// authority ID will return a file with a name in the format "2015_AA_producerdetails_31122016_2359"
        /// containing the specified compliance year, the authority abbreviation and the current time.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task GetDownloadProducerDetailsCsv_WithNoSchemeIdAndWithAnAuthorityId_ReturnsFileNameWithComplianceYearAuthorityAbbreviationAndCurrentTime()
        {
            // Arrange
            IWeeeClient client = A.Fake<IWeeeClient>();

            CSVFileData file = new CSVFileData() { FileContent = "Content" };
            A.CallTo(() => client.SendAsync(A<string>._, A<GetMemberDetailsCSV>._))
                .Returns(file);

            UKCompetentAuthorityData authorityData = new UKCompetentAuthorityData() { Abbreviation = "AA" };
            A.CallTo(() => client.SendAsync(A<string>._, A<GetUKCompetentAuthorityById>._))
                .WhenArgumentsMatch(a => a.Get<GetUKCompetentAuthorityById>("request").Id == new Guid("703839B3-A081-4491-92B7-FCF969067EA3"))
                .Returns(authorityData);

            ReportsController controller = new ReportsController(
                () => client,
                A.Dummy<BreadcrumbService>());

            // Act
            SystemTime.Freeze(new DateTime(2016, 12, 31, 23, 59, 58));
            ActionResult result = await controller.DownloadProducerDetailsCsv(
                2015,
                null,
                new Guid("703839B3-A081-4491-92B7-FCF969067EA3"),
                false);
            SystemTime.Unfreeze();

            // Assert
            FileResult fileResult = result as FileResult;
            Assert.NotNull(fileResult);

            Assert.Equal("2015_AA_producerdetails_31122016_2359.csv", fileResult.FileDownloadName);
        }

        /// <summary>
        /// This test ensures that the GET "DownloadProducerDetailsCsv" action will
        /// return a FileResult with a content type of "text/csv".
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task GetDownloadProducerDetailsCsv_Always_ReturnsFileResultWithContentTypeOfTextCsv()
        {
            // Arrange
            IWeeeClient client = A.Fake<IWeeeClient>();

            CSVFileData file = new CSVFileData() { FileContent = "Content" };
            A.CallTo(() => client.SendAsync(A<string>._, A<GetMemberDetailsCSV>._))
                .Returns(file);

            ReportsController controller = new ReportsController(
                () => client,
                A.Dummy<BreadcrumbService>());

            // Act
            ActionResult result = await controller.DownloadProducerDetailsCsv(2015, null, null, false);

            // Assert
            FileResult fileResult = result as FileResult;
            Assert.NotNull(fileResult);

            Assert.Equal("text/csv", fileResult.ContentType);
        }

        [Fact]
        public async void HttpGet_UKEEEData_ShouldReturnsUKEEEDataView()
        {
            IWeeeClient client = A.Fake<IWeeeClient>();
            //var controller = ReportsController();
            ReportsController controller = new ReportsController(
                () => client,
                A.Dummy<BreadcrumbService>());

            A.CallTo(() => client.SendAsync(A<string>._, A<GetDataReturnsActiveComplianceYears>._))
                .Returns(new List<int> { 2015, 2016 });

            var result = await controller.UKEEEData();

            var viewResult = ((ViewResult)result);
            Assert.Equal("UKEEEData", viewResult.ViewName);
        }

        [Fact]
        public async Task GetUKEeeData_Always_SetsTriggerDownloadToFalse()
        {
            // Arrange
            ReportsController controller = new ReportsController(
                () => A.Dummy<IWeeeClient>(),
                A.Dummy<BreadcrumbService>());

            // Act
            ActionResult result = await controller.UKEEEData();

            // Assert
            ViewResult viewResult = result as ViewResult;
            Assert.NotNull(viewResult);
            Assert.Equal(false, viewResult.ViewBag.TriggerDownload);
        }

        [Fact]
        public async void HttpPost_UKEEEData_ModelIsInvalid_ShouldRedirectViewWithError()
        {
            IWeeeClient client = A.Fake<IWeeeClient>();
            ReportsController controller = new ReportsController(
                () => client,
                A.Dummy<BreadcrumbService>());
            controller.ModelState.AddModelError("Key", "Any error");

            var result = await controller.UKEEEData(new UKEEEDataViewModel());

            Assert.IsType<ViewResult>(result);
            Assert.False(controller.ModelState.IsValid);
        }

        [Fact]
        public async Task PostUKEeeData_ModelIsInvalid_SetsTriggerDownloadToFalse()
        {
            // Arrange
            ReportsController controller = new ReportsController(
                () => A.Dummy<IWeeeClient>(),
                A.Dummy<BreadcrumbService>());

            UKEEEDataViewModel viewModel = new UKEEEDataViewModel();

            // Act
            controller.ModelState.AddModelError("Key", "Error");
            ActionResult result = await controller.UKEEEData(viewModel);

            // Assert
            ViewResult viewResult = result as ViewResult;
            Assert.NotNull(viewResult);
            Assert.Equal(false, viewResult.ViewBag.TriggerDownload);
        }

        [Fact]
        public async Task PostUKEeeData_ValidModel_SetsTriggerDownloadToTrue()
        {
            // Arrange
            ReportsController controller = new ReportsController(
                () => A.Dummy<IWeeeClient>(),
                A.Dummy<BreadcrumbService>());

            UKEEEDataViewModel viewModel = new UKEEEDataViewModel();

            // Act
            ActionResult result = await controller.UKEEEData(viewModel);

            // Assert
            ViewResult viewResult = result as ViewResult;
            Assert.NotNull(viewResult);
            Assert.Equal(true, viewResult.ViewBag.TriggerDownload);
        }

        [Fact]
        public async Task GetDownloadUkEeeDataCsv_ReturnsFileResultWithContentTypeOfTextCsv()
        {
            // Arrange
            IWeeeClient weeeClient = A.Fake<IWeeeClient>();
            A.CallTo(() => weeeClient.SendAsync(A<string>._, A<GetUkEeeDataCsv>._)).Returns(new CSVFileData
            {
                FileContent = "UK EEE DATA REPORT",
                FileName = "test.csv"
            });
            
            ReportsController controller = new ReportsController(
                () => weeeClient,
                A.Dummy<BreadcrumbService>());

            // Act
            ActionResult result = await controller.DownloadUkEeeDataCsv(2015);

            // Assert
            FileResult fileResult = result as FileResult;
            Assert.NotNull(fileResult);
            Assert.Equal("text/csv", fileResult.ContentType);
        }

        /// <summary>
        /// This test ensures that the GET "SchemeWeeeData" action calls the API to retrieve
        /// the list of compliance years and returns the "SchemeWeeeData" view with 
        /// a ProducerDataViewModel that has be populated with the list of years.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task GetSchemeWeeeData_Always_ReturnsSchemeWeeeDataProducerDataViewModel()
        {
            // Arrange
            List<int> years = new List<int>() { 2001, 2002 };

            IWeeeClient weeeClient = A.Fake<IWeeeClient>();
            A.CallTo(() => weeeClient.SendAsync(A<string>._, A<GetDataReturnsActiveComplianceYears>._)).Returns(years);

            ReportsController controller = new ReportsController(
                () => weeeClient,
                A.Dummy<BreadcrumbService>());

            // Act
            ActionResult result = await controller.SchemeWeeeData();

            // Assert
            ViewResult viewResult = result as ViewResult;
            Assert.NotNull(viewResult);
            Assert.True(string.IsNullOrEmpty(viewResult.ViewName) || viewResult.ViewName == "SchemeWeeeData");

            ProducersDataViewModel model = viewResult.Model as ProducersDataViewModel;
            Assert.NotNull(model);
            Assert.Collection(model.ComplianceYears,
                y1 => Assert.Equal("2001", y1.Text),
                y2 => Assert.Equal("2002", y2.Text));
        }

        /// <summary>
        /// This test ensures that the GET "SchemeWeeeData" action returns
        /// a view with the ViewBag property "TriggerDownload" set to false.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task GetSchemeWeeeData_Always_SetsTriggerDownloadToFalse()
        {
            // Arrange
            ReportsController controller = new ReportsController(
                () => A.Dummy<IWeeeClient>(),
                A.Dummy<BreadcrumbService>());

            // Act
            ActionResult result = await controller.SchemeWeeeData();

            // Assert
            ViewResult viewResult = result as ViewResult;
            Assert.NotNull(viewResult);
            Assert.Equal(false, viewResult.ViewBag.TriggerDownload);
        }

        /// <summary>
        /// This test ensures that the GET "SchemeWeeeData" action sets
        /// the breadcrumb's internal activity to "View reports".
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task GetSchemeWeeeData_Always_SetsInternalBreadcrumbToViewReports()
        {
            BreadcrumbService breadcrumb = new BreadcrumbService();

            // Arrange
            ReportsController controller = new ReportsController(
                () => A.Dummy<IWeeeClient>(),
                breadcrumb);

            // Act
            ActionResult result = await controller.SchemeWeeeData();

            // Assert
            Assert.Equal("View reports", breadcrumb.InternalActivity);
        }

        /// <summary>
        /// This test ensures that the POST "SchemeWeeeData" action with an invalid view model
        /// calls the API to retrieve the list of compliance years and returns the "SchemeWeeeData"
        /// view with a ProducerDataViewModel that has be populated with the list of years.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task PostSchemeWeeeData_WithInvalidViewModel_ReturnsSchemeWeeeDataProducerDataViewModel()
        {
            // Arrange
            List<int> years = new List<int>() { 2001, 2002 };

            IWeeeClient weeeClient = A.Fake<IWeeeClient>();
            A.CallTo(() => weeeClient.SendAsync(A<string>._, A<GetDataReturnsActiveComplianceYears>._)).Returns(years);

            ReportsController controller = new ReportsController(
                () => weeeClient,
                A.Dummy<BreadcrumbService>());

            ProducersDataViewModel viewModel = new ProducersDataViewModel();

            // Act
            controller.ModelState.AddModelError("Key", "Error");
            ActionResult result = await controller.SchemeWeeeData(viewModel);

            // Assert
            ViewResult viewResult = result as ViewResult;
            Assert.NotNull(viewResult);
            Assert.True(string.IsNullOrEmpty(viewResult.ViewName) || viewResult.ViewName == "SchemeWeeeData");

            ProducersDataViewModel model = viewResult.Model as ProducersDataViewModel;
            Assert.NotNull(model);
            Assert.Collection(model.ComplianceYears,
                y1 => Assert.Equal("2001", y1.Text),
                y2 => Assert.Equal("2002", y2.Text));
        }

        /// <summary>
        /// This test ensures that the POST "SchemeWeeeData" action with an invalid view model
        /// returns a view with the ViewBag property "TriggerDownload" set to false.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task PostSchemeWeeeData_WithInvalidViewModel_SetsTriggerDownloadToFalse()
        {
            // Arrange
            ReportsController controller = new ReportsController(
                () => A.Dummy<IWeeeClient>(),
                A.Dummy<BreadcrumbService>());

            ProducersDataViewModel viewModel = new ProducersDataViewModel();

            // Act
            controller.ModelState.AddModelError("Key", "Error");
            ActionResult result = await controller.SchemeWeeeData(viewModel);

            // Assert
            ViewResult viewResult = result as ViewResult;
            Assert.NotNull(viewResult);
            Assert.Equal(false, viewResult.ViewBag.TriggerDownload);
        }

        /// <summary>
        /// This test ensures that the POST "SchemeWeeeData" action with a valid view model
        /// returns a view with the ViewBag property "TriggerDownload" set to true.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task PostSchemeWeeeData_WithViewModel_SetsTriggerDownloadToTrue()
        {
            // Arrange
            ReportsController controller = new ReportsController(
                () => A.Dummy<IWeeeClient>(),
                A.Dummy<BreadcrumbService>());

            ProducersDataViewModel viewModel = new ProducersDataViewModel();

            // Act
            ActionResult result = await controller.SchemeWeeeData(viewModel);

            // Assert
            ViewResult viewResult = result as ViewResult;
            Assert.NotNull(viewResult);
            Assert.Equal(true, viewResult.ViewBag.TriggerDownload);
        }

        /// <summary>
        /// This test ensures that the POST "SchemeWeeeData" action sets
        /// the breadcrumb's internal activity to "View reports".
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task PostSchemeWeeeData_Always_SetsInternalBreadcrumbToViewReports()
        {
            BreadcrumbService breadcrumb = new BreadcrumbService();

            // Arrange
            ReportsController controller = new ReportsController(
                () => A.Dummy<IWeeeClient>(),
                breadcrumb);

            // Act
            ActionResult result = await controller.SchemeWeeeData(A.Dummy<ProducersDataViewModel>());

            // Assert
            Assert.Equal("View reports", breadcrumb.InternalActivity);
        }

        /// <summary>
        /// This test ensures that the GET "DownloadSchemeWeeeDataCsv" action will
        /// call the API to generate a CSV file which is returned with the correct file name.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task GetDownloadSchemeWeeeDataCsv_Always_CallsApiAndReturnsFileResultWithCorrectFileName()
        {
            // Arrange
            FileInfo file = new FileInfo("TEST FILE.csv", new byte[] { 1, 2, 3 });

            IWeeeClient weeeClient = A.Fake<IWeeeClient>();
            A.CallTo(() => weeeClient.SendAsync(A<string>._, A<GetSchemeWeeeCsv>._))
                .WhenArgumentsMatch(a =>
                    a.Get<GetSchemeWeeeCsv>("request").ComplianceYear == 2015 &&
                    a.Get<GetSchemeWeeeCsv>("request").ObligationType == ObligationType.B2C)
                .Returns(file);

            ReportsController controller = new ReportsController(
                () => weeeClient,
                A.Dummy<BreadcrumbService>());

            ProducersDataViewModel viewModel = new ProducersDataViewModel();

            // Act
            ActionResult result = await controller.DownloadSchemeWeeeDataCsv(2015, ObligationType.B2C);

            // Assert
            FileResult fileResult = result as FileResult;
            Assert.NotNull(fileResult);

            Assert.Equal("TEST FILE.csv", fileResult.FileDownloadName);
        }

        /// <summary>
        /// This test ensures that the GET "DownloadSchemeWeeeDataCsv" action will
        /// call the API to generate a CSV file which is returned with a content
        /// type of "text/csv"
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task GetDownloadSchemeWeeeDataCsv_Always_CallsApiAndReturnsFileResultWithContentTypeOfTextCsv()
        {
            // Arrange
            IWeeeClient weeeClient = A.Fake<IWeeeClient>();
            A.CallTo(() => weeeClient.SendAsync(A<GetSchemeWeeeCsv>._))
                .WhenArgumentsMatch(a => a.Get<int>("complianceYear") == 2015 && a.Get<ObligationType>("obligationType") == ObligationType.B2C)
                .Returns(A.Dummy<FileInfo>());

            ReportsController controller = new ReportsController(
                () => weeeClient,
                A.Dummy<BreadcrumbService>());

            ProducersDataViewModel viewModel = new ProducersDataViewModel();

            // Act
            ActionResult result = await controller.DownloadSchemeWeeeDataCsv(2015, ObligationType.B2C);

            // Assert
            FileResult fileResult = result as FileResult;
            Assert.NotNull(fileResult);

            Assert.Equal("text/csv", fileResult.ContentType);
        }

        /// <summary>
        /// This test ensures that the GET "UKWeeeData" action calls the API to retrieve
        /// the list of compliance years and returns the "UKWeeeData" view with 
        /// a ProducerDataViewModel that has be populated with the list of years.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task GetUKWeeeData_Always_ReturnsUKWeeeDataProducerDataViewModel()
        {
            // Arrange
            List<int> years = new List<int>() { 2001, 2002 };

            IWeeeClient weeeClient = A.Fake<IWeeeClient>();
            A.CallTo(() => weeeClient.SendAsync(A<string>._, A<GetDataReturnsActiveComplianceYears>._)).Returns(years);

            ReportsController controller = new ReportsController(
                () => weeeClient,
                A.Dummy<BreadcrumbService>());

            // Act
            ActionResult result = await controller.UKWeeeData();

            // Assert
            ViewResult viewResult = result as ViewResult;
            Assert.NotNull(viewResult);
            Assert.True(string.IsNullOrEmpty(viewResult.ViewName) || viewResult.ViewName == "UKWeeeData");

            ProducersDataViewModel model = viewResult.Model as ProducersDataViewModel;
            Assert.NotNull(model);
            Assert.Collection(model.ComplianceYears,
                y1 => Assert.Equal("2001", y1.Text),
                y2 => Assert.Equal("2002", y2.Text));
        }

        /// <summary>
        /// This test ensures that the GET "UKWeeeData" action returns
        /// a view with the ViewBag property "TriggerDownload" set to false.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task GetUKWeeeData_Always_SetsTriggerDownloadToFalse()
        {
            // Arrange
            ReportsController controller = new ReportsController(
                () => A.Dummy<IWeeeClient>(),
                A.Dummy<BreadcrumbService>());

            // Act
            ActionResult result = await controller.UKWeeeData();

            // Assert
            ViewResult viewResult = result as ViewResult;
            Assert.NotNull(viewResult);
            Assert.Equal(false, viewResult.ViewBag.TriggerDownload);
        }

        /// <summary>
        /// This test ensures that the GET "UKWeeeData" action sets
        /// the breadcrumb's internal activity to "View reports".
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task GetUKWeeeData_Always_SetsInternalBreadcrumbToViewReports()
        {
            BreadcrumbService breadcrumb = new BreadcrumbService();

            // Arrange
            ReportsController controller = new ReportsController(
                () => A.Dummy<IWeeeClient>(),
                breadcrumb);

            // Act
            ActionResult result = await controller.UKWeeeData();

            // Assert
            Assert.Equal("View reports", breadcrumb.InternalActivity);
        }

        /// <summary>
        /// This test ensures that the POST "UKWeeeData" action with an invalid view model
        /// calls the API to retrieve the list of compliance years and returns the "UKWeeeData"
        /// view with a ProducerDataViewModel that has be populated with the list of years.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task PostUKWeeeData_WithInvalidViewModel_ReturnsUKWeeeDataProducerDataViewModel()
        {
            // Arrange
            List<int> years = new List<int>() { 2001, 2002 };

            IWeeeClient weeeClient = A.Fake<IWeeeClient>();
            A.CallTo(() => weeeClient.SendAsync(A<string>._, A<GetDataReturnsActiveComplianceYears>._)).Returns(years);

            ReportsController controller = new ReportsController(
                () => weeeClient,
                A.Dummy<BreadcrumbService>());

            ProducersDataViewModel viewModel = new ProducersDataViewModel();

            // Act
            controller.ModelState.AddModelError("Key", "Error");
            ActionResult result = await controller.UKWeeeData(viewModel);

            // Assert
            ViewResult viewResult = result as ViewResult;
            Assert.NotNull(viewResult);
            Assert.True(string.IsNullOrEmpty(viewResult.ViewName) || viewResult.ViewName == "UKWeeeData");

            ProducersDataViewModel model = viewResult.Model as ProducersDataViewModel;
            Assert.NotNull(model);
            Assert.Collection(model.ComplianceYears,
                y1 => Assert.Equal("2001", y1.Text),
                y2 => Assert.Equal("2002", y2.Text));
        }

        /// <summary>
        /// This test ensures that the POST "UKWeeeData" action with an invalid view model
        /// returns a view with the ViewBag property "TriggerDownload" set to false.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task PostUKWeeeData_WithInvalidViewModel_SetsTriggerDownloadToFalse()
        {
            // Arrange
            ReportsController controller = new ReportsController(
                () => A.Dummy<IWeeeClient>(),
                A.Dummy<BreadcrumbService>());

            ProducersDataViewModel viewModel = new ProducersDataViewModel();

            // Act
            controller.ModelState.AddModelError("Key", "Error");
            ActionResult result = await controller.UKWeeeData(viewModel);

            // Assert
            ViewResult viewResult = result as ViewResult;
            Assert.NotNull(viewResult);
            Assert.Equal(false, viewResult.ViewBag.TriggerDownload);
        }

        /// <summary>
        /// This test ensures that the POST "UKWeeeData" action with a valid view model
        /// returns a view with the ViewBag property "TriggerDownload" set to true.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task PostUKWeeeData_WithViewModel_SetsTriggerDownloadToTrue()
        {
            // Arrange
            ReportsController controller = new ReportsController(
                () => A.Dummy<IWeeeClient>(),
                A.Dummy<BreadcrumbService>());

            ProducersDataViewModel viewModel = new ProducersDataViewModel();

            // Act
            ActionResult result = await controller.UKWeeeData(viewModel);

            // Assert
            ViewResult viewResult = result as ViewResult;
            Assert.NotNull(viewResult);
            Assert.Equal(true, viewResult.ViewBag.TriggerDownload);
        }

        /// <summary>
        /// This test ensures that the POST "UKWeeeData" action sets
        /// the breadcrumb's internal activity to "View reports".
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task PostUKWeeeData_Always_SetsInternalBreadcrumbToViewReports()
        {
            BreadcrumbService breadcrumb = new BreadcrumbService();

            // Arrange
            ReportsController controller = new ReportsController(
                () => A.Dummy<IWeeeClient>(),
                breadcrumb);

            // Act
            ActionResult result = await controller.UKWeeeData(A.Dummy<ProducersDataViewModel>());

            // Assert
            Assert.Equal("View reports", breadcrumb.InternalActivity);
        }

        /// <summary>
        /// This test ensures that the GET "DownloadUKWeeeDataCsv" action will
        /// call the API to generate a CSV file which is returned with the correct file name.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task GetDownloadUKWeeeDataCsv_Always_CallsApiAndReturnsFileResultWithCorrectFileName()
        {
            // Arrange
            FileInfo file = new FileInfo("TEST FILE.csv", new byte[] { 1, 2, 3 });

            IWeeeClient weeeClient = A.Fake<IWeeeClient>();
            A.CallTo(() => weeeClient.SendAsync(A<string>._, A<GetUKWeeeCsv>._))
                .WhenArgumentsMatch(a => a.Get<GetUKWeeeCsv>("request").ComplianceYear == 2015)
                .Returns(file);

            ReportsController controller = new ReportsController(
                () => weeeClient,
                A.Dummy<BreadcrumbService>());

            ProducersDataViewModel viewModel = new ProducersDataViewModel();

            // Act
            ActionResult result = await controller.DownloadUKWeeeDataCsv(2015);

            // Assert
            FileResult fileResult = result as FileResult;
            Assert.NotNull(fileResult);

            Assert.Equal("TEST FILE.csv", fileResult.FileDownloadName);
        }

        /// <summary>
        /// This test ensures that the GET "DownloadUKWeeeDataCsv" action will
        /// call the API to generate a CSV file which is returned with a content
        /// type of "text/csv"
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task GetDownloadUKWeeeDataCsv_Always_CallsApiAndReturnsFileResultWithContentTypeOfTextCsv()
        {
            // Arrange
            IWeeeClient weeeClient = A.Fake<IWeeeClient>();
            A.CallTo(() => weeeClient.SendAsync(A<GetUKWeeeCsv>._))
                .WhenArgumentsMatch(a => a.Get<int>("complianceYear") == 2015)
                .Returns(A.Dummy<FileInfo>());

            ReportsController controller = new ReportsController(
                () => weeeClient,
                A.Dummy<BreadcrumbService>());

            ProducersDataViewModel viewModel = new ProducersDataViewModel();

            // Act
            ActionResult result = await controller.DownloadUKWeeeDataCsv(2015);

            // Assert
            FileResult fileResult = result as FileResult;
            Assert.NotNull(fileResult);

            Assert.Equal("text/csv", fileResult.ContentType);
        }
    }
}
