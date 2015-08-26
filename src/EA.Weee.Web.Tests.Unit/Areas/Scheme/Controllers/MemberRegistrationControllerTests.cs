﻿namespace EA.Weee.Web.Tests.Unit.Areas.Scheme.Controllers
{
    using Api.Client;
    using Core.Scheme;
    using Core.Shared;
    using EA.Weee.Web.Services.Caching;
    using FakeItEasy;
    using Prsd.Core.Mediator;
    using Services;
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using System.Web;
    using System.Web.Mvc;
    using TestHelpers;
    using Web.Areas.Scheme.Controllers;
    using Web.Areas.Scheme.ViewModels;
    using Weee.Requests.Organisations;
    using Weee.Requests.Scheme;
    using Weee.Requests.Scheme.MemberRegistration;
    using Xunit;

    public class MemberRegistrationControllerTests
    {
        private readonly IWeeeClient weeeClient;
        private readonly IFileConverterService fileConverter;

        public MemberRegistrationControllerTests()
        {
            weeeClient = A.Fake<IWeeeClient>();
            fileConverter = A.Fake<IFileConverterService>();
        }

        private MemberRegistrationController MemberRegistrationController()
        {
            var controller = new MemberRegistrationController(
                () => weeeClient,
                fileConverter,
                A.Fake<IWeeeCache>(),
                A.Fake<BreadcrumbService>());

            new HttpContextMocker().AttachToController(controller);

            return controller;
        }

        private FakeMemberRegistrationController BuildFakeMemberRegistrationController()
        {
            var controller = new FakeMemberRegistrationController
                (weeeClient,
                fileConverter,           
                A.Fake<IWeeeCache>(),
                A.Fake<BreadcrumbService>());

            new HttpContextMocker().AttachToController(controller);

            return controller;
        }

        [Fact]
        public async void GetAuthorizationRequired_ChecksStatusOfScheme()
        {
            await MemberRegistrationController().AuthorizationRequired(A<Guid>._);

            A.CallTo(() => weeeClient.SendAsync(A<string>._, A<GetSchemeStatus>._))
                .MustHaveHappened(Repeated.Exactly.Once);
        }

        [Fact]
        public async void GetAuthorizationRequired_SchemeIsPendingApproval_ReturnsViewWithPendingStatus()
        {
            A.CallTo(() => weeeClient.SendAsync(A<string>._, A<GetSchemeStatus>._))
                .Returns(SchemeStatus.Pending);

            var result = await MemberRegistrationController().AuthorizationRequired(A<Guid>._);

            Assert.IsType<ViewResult>(result);

            var view = ((AuthorizationRequiredViewModel)((ViewResult)result).Model);

            Assert.Equal(SchemeStatus.Pending, view.Status);
        }

        [Fact]
        public async void GetAuthorizationRequired_SchemeIsRejected_ReturnsViewWithRejectedStatus()
        {
            A.CallTo(() => weeeClient.SendAsync(A<string>._, A<GetSchemeStatus>._))
                .Returns(SchemeStatus.Rejected);

            var result = await MemberRegistrationController().AuthorizationRequired(A<Guid>._);

            Assert.IsType<ViewResult>(result);

            var view = ((AuthorizationRequiredViewModel)((ViewResult)result).Model);

            Assert.Equal(SchemeStatus.Rejected, view.Status);
        }

        [Fact]
        public async void GetAuthorizationRequired_SchemeIsApproved_RedirectsToPcsMemberSummary()
        {
            A.CallTo(() => weeeClient.SendAsync(A<string>._, A<GetSchemeStatus>._))
                .Returns(SchemeStatus.Approved);

            var result = await MemberRegistrationController().AuthorizationRequired(A<Guid>._);

            Assert.IsType<RedirectToRouteResult>(result);

            var routeValues = ((RedirectToRouteResult)result).RouteValues;

            Assert.Equal("Summary", routeValues["action"]);
            Assert.Equal("MemberRegistration", routeValues["controller"]);
        }

        [Fact]
        public async void GetAddOrAmendMembers_ChecksForValidityOfOrganisation()
        {
            try
            {
                await MemberRegistrationController().AddOrAmendMembers(A<Guid>._);
            }
            catch (Exception)
            {
            }

            A.CallTo(() => weeeClient.SendAsync(A<string>._, A<VerifyOrganisationExists>._))
                .MustHaveHappened(Repeated.Exactly.Once);
        }

        [Fact]
        public async void GetAddOrAmendMembers_IdDoesNotBelongToAnExistingOrganisation_ThrowsException()
        {
            A.CallTo(() => weeeClient.SendAsync(A<string>._, A<VerifyOrganisationExists>._))
                .Returns(false);

            await Assert.ThrowsAnyAsync<Exception>(() => MemberRegistrationController().AddOrAmendMembers(A<Guid>._));
        }

        [Fact]
        public async void GetAddOrAmendMembers_IdDoesBelongToAnExistingOrganisation_ReturnsView()
        {
            A.CallTo(() => weeeClient.SendAsync(A<string>._, A<VerifyOrganisationExists>._))
                .Returns(true);

            var result = await MemberRegistrationController().AddOrAmendMembers(A<Guid>._);

            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public async void PostAddOrAmendMembers_ModelIsInvalid_ReturnsView()
        {
            var controller = MemberRegistrationController();
            controller.ModelState.AddModelError("ErrorKey", "Some kind of error goes here");

            var result = await controller.AddOrAmendMembers(A<Guid>._, new AddOrAmendMembersViewModel());

            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public async void PostAddOrAmendMembers_ConvertsFileToString()
        {
            try
            {
                await MemberRegistrationController().AddOrAmendMembers(A<Guid>._, new AddOrAmendMembersViewModel());
            }
            catch (Exception)
            {
            }

            A.CallTo(() => fileConverter.Convert(A<HttpPostedFileBase>._))
                .MustHaveHappened(Repeated.Exactly.Once);
        }

        [Fact]
        public async void PostAddOrAmendMembers_FileIsConvertedSuccessfully_ValidateRequestSentWithConvertedFileDataAndOrganisationId()
        {
            var fileData = new byte[1];
            var organisationId = Guid.NewGuid();
            var request = new ProcessXMLFile(A<Guid>._, A<byte[]>._);

            A.CallTo(() => fileConverter.Convert(A<HttpPostedFileBase>._))
                .Returns(fileData);

            A.CallTo(() => weeeClient.SendAsync(A<string>._, A<ProcessXMLFile>._))
                .Invokes((string token, IRequest<Guid> req) => request = (ProcessXMLFile)req);

            try
            {
                await MemberRegistrationController().AddOrAmendMembers(organisationId, new AddOrAmendMembersViewModel());
            }
            catch (Exception)
            {
            }

            A.CallTo(() => weeeClient.SendAsync(A<string>._, A<ProcessXMLFile>._))
                .MustHaveHappened(Repeated.Exactly.Once);

            Assert.NotNull(request);
            Assert.Equal(fileData, request.Data);
            Assert.Equal(organisationId, request.OrganisationId);
        }

        [Fact]
        public async void PostAddOrAmendMembers_ValidateRequestIsProcessedSuccessfully_RedirectsToResults()
        {
            var validationId = Guid.NewGuid();

            A.CallTo(() => weeeClient.SendAsync(A<string>._, A<ProcessXMLFile>._))
                .Returns(validationId);

            var result = await MemberRegistrationController().AddOrAmendMembers(A<Guid>._, new AddOrAmendMembersViewModel());
            var redirect = (RedirectToRouteResult)result;

            Assert.Equal("ViewErrorsAndWarnings", redirect.RouteValues["action"]);
            Assert.Equal("MemberRegistration", redirect.RouteValues["controller"]);
            Assert.Equal(validationId, redirect.RouteValues["memberUploadId"]);
        }

        [Fact]
        public async void GetSummary_GetsSummaryOfLatestMemberUpload()
        {
            await MemberRegistrationController().Summary(A<Guid>._);

            A.CallTo(() => weeeClient.SendAsync(A<string>._, A<GetComplianceYears>._))
                .MustHaveHappened(Repeated.Exactly.Once);
        }

        [Fact]
        public async void GetSummary_HasNoUploads_RedirectsToAddOrAmendMembersPage()
        {
            A.CallTo(() => weeeClient.SendAsync(A<string>._, A<GetComplianceYears>._))
                .Returns(new List<int>());

            var result = await MemberRegistrationController().Summary(A<Guid>._);

            Assert.IsType<RedirectToRouteResult>(result);

            var routeValues = ((RedirectToRouteResult)result).RouteValues;

            Assert.Equal("AddOrAmendMembers", routeValues["action"]);
            Assert.Equal("MemberRegistration", routeValues["controller"]);
        }

        [Fact]
        public async void GetSummary_HasUploadForThisScheme_ReturnsViewWithSummaryModel()
        {
            A.CallTo(() => weeeClient.SendAsync(A<string>._, A<GetComplianceYears>._))
                .Returns(new List<int>() { 2015, 2016 });

            var result = await MemberRegistrationController().Summary(A<Guid>._);

            Assert.IsType<ViewResult>(result);
            Assert.IsType<List<int>>(((ViewResult)result).Model);
        }

        private const string XmlHasErrorsViewName = "ViewErrorsAndWarnings";
        private const string XmlHasNoErrorsViewName = "XmlHasNoErrors";

        [Fact]
        public async Task GetViewErrorsOrWarnings_NoErrors_ShowsAcceptedPage()
        {
            Assert.Equal(XmlHasNoErrorsViewName, await ViewAfterClientReturns(new List<MemberUploadErrorData> { }));
        }

        [Fact]
        public async Task GetViewErrorsOrWarnings_ErrorsPresent_ShowsErrorPage()
        {
            Assert.Equal(XmlHasErrorsViewName, await ViewAfterClientReturns(new List<MemberUploadErrorData> { new MemberUploadErrorData { ErrorLevel = ErrorLevel.Error } }));
        }

        [Fact]
        public async Task GetViewErrorsOrWarnings_WarningPresent_ShowsAcceptedPage()
        {
            Assert.Equal(XmlHasNoErrorsViewName, await ViewAfterClientReturns(new List<MemberUploadErrorData> { new MemberUploadErrorData { ErrorLevel = ErrorLevel.Warning } }));
        }

        [Fact]
        public async Task GetViewErrorsOrWarnings_NoErrors_HasProvidedErrorData()
        {
            var errors = new List<MemberUploadErrorData>();

            var providedErrors = await ErrorsAfterClientReturns(errors);

            Assert.Equal(errors, providedErrors);
        }

        [Fact]
        public async Task GetViewErrorsOrWarnings_ErrorsPresent_HasProvidedErrorData()
        {
            var errors = new List<MemberUploadErrorData>
            {
                new MemberUploadErrorData
                {
                    ErrorLevel = ErrorLevel.Error
                }
            };

            var providedErrors = await ErrorsAfterClientReturns(errors);

            Assert.Equal(errors, providedErrors);
        }

        [Fact]
        public async void GetProducerCSV_ValidComplianceYear_ReturnsCSVFile()
        {
            var testCSVData = new ProducerCSVFileData { FileContent = "Test, Test, Test", FileName = "test.csv" };

            A.CallTo(() => weeeClient.SendAsync(A<string>._, A<GetProducerCSV>._))
                .Returns(testCSVData);

            var result = await MemberRegistrationController().GetProducerCSV(A<Guid>._, A<int>._);

            Assert.IsType<FileContentResult>(result);
        }

        [Fact]
        public async void PostSubmitXml_ValidMemberUploadId_ReturnsSuccessfulSubmissionView()
        {
            var memberUploadId = Guid.NewGuid();
            A.CallTo(() => weeeClient.SendAsync(A<string>._, A<MemberUploadSubmission>._))
                .Returns(memberUploadId);

            var result = await MemberRegistrationController().SubmitXml(A<Guid>._, new MemberUploadResultViewModel { ErrorData = new List<MemberUploadErrorData>(), MemberUploadId = memberUploadId });

            var redirect = (RedirectToRouteResult)result;

            Assert.Equal("SuccessfulSubmission", redirect.RouteValues["action"]);
            Assert.Equal(memberUploadId, redirect.RouteValues["memberUploadId"]);
        }

        [Fact]
        public void OnActionExecuting_ActionAuthorizationRequired_DoesNotCheckPcsId()
        {
            var fakeController = BuildFakeMemberRegistrationController();
            var fakeActionParameters = ActionExecutingContextHelper.FakeActionParameters();
            var fakeActionDescriptor = ActionExecutingContextHelper.FakeActionDescriptorWithActionName("AuthorizationRequired");

            ActionExecutingContext context = new ActionExecutingContext();
            context.ActionParameters = fakeActionParameters;
            context.ActionDescriptor = fakeActionDescriptor;

            fakeController.InvokeOnActionExecuting(context);

            A.CallTo(() => fakeActionDescriptor.ActionName).MustHaveHappened(Repeated.Exactly.Once);

            object dummyObject;
            A.CallTo(() => fakeActionParameters.TryGetValue(A<string>._, out dummyObject)).MustNotHaveHappened();
        }

        [Fact]
        public void OnActionExecuting_NotActionAuthorizationRequiredNoPcs_ThrowsInvalidOperationException()
        {
            var fakeController = BuildFakeMemberRegistrationController();
            var fakeActionParameters = ActionExecutingContextHelper.FakeActionParameters(false, A.Dummy<Guid>());

            ActionExecutingContext context = new ActionExecutingContext();
            context.ActionParameters = fakeActionParameters;
            context.ActionDescriptor = ActionExecutingContextHelper.FakeActionDescriptorWithActionName("TestAction");

            Assert.Throws(typeof(InvalidOperationException), () => fakeController.InvokeOnActionExecuting(context));
        }

        [Fact]
        public void OnActionExecuting_NotActionAuthorizationRequiredNotApprovedPcs_ResultsToAuthorizationRequired()
        {
            var pcsId = Guid.NewGuid();
            var fakeController = BuildFakeMemberRegistrationController();
            var fakeActionParameters = ActionExecutingContextHelper.FakeActionParameters(true, pcsId);

            ActionExecutingContext context = new ActionExecutingContext();
            context.ActionParameters = fakeActionParameters;
            context.ActionDescriptor = ActionExecutingContextHelper.FakeActionDescriptorWithActionName("TestAction");

            A.CallTo(() => weeeClient.SendAsync(A<string>._, A<GetSchemeStatus>._))
                .Returns(SchemeStatus.Pending);

            fakeController.InvokeOnActionExecuting(context);

            var redirect = (RedirectToRouteResult)context.Result;

            Assert.Equal("AuthorizationRequired", redirect.RouteValues["action"]);
            Assert.Equal(pcsId, redirect.RouteValues["pcsId"]);
        }

        [Fact]
        public void OnActionExecuting_NotActionAuthorizationRequiredApprovedPcs_ResultsToNoRedirection()
        {
            var fakeController = BuildFakeMemberRegistrationController();
            var fakeActionParameters = ActionExecutingContextHelper.FakeActionParameters(true, A.Dummy<Guid>());

            ActionExecutingContext context = new ActionExecutingContext();
            context.ActionParameters = fakeActionParameters;
            context.ActionDescriptor = ActionExecutingContextHelper.FakeActionDescriptorWithActionName("TestAction");

            A.CallTo(() => weeeClient.SendAsync(A<string>._, A<GetSchemeStatus>._))
                .Returns(SchemeStatus.Approved);

            fakeController.InvokeOnActionExecuting(context);

            A.CallTo(() => weeeClient.SendAsync(A<string>._, A<GetSchemeStatus>._))
                .MustHaveHappened();

            Assert.Null(context.Result);
        }

        private async Task<List<MemberUploadErrorData>> ErrorsAfterClientReturns(List<MemberUploadErrorData> memberUploadErrorDatas)
        {
            var result = await GetViewResult(memberUploadErrorDatas);

            return ((MemberUploadResultViewModel)result.Model).ErrorData;
        }

        private async Task<string> ViewAfterClientReturns(List<MemberUploadErrorData> list)
        {
            return (await GetViewResult(list)).ViewName;
        }

        private async Task<ViewResult> GetViewResult(List<MemberUploadErrorData> memberUploadErrorDatas)
        {
            A.CallTo(() => weeeClient.SendAsync(A<string>._, A<GetMemberUploadData>._))
                .Returns(memberUploadErrorDatas);

            return await MemberRegistrationController().ViewErrorsAndWarnings(A<Guid>._, A<Guid>._);
        }

        private class FakeMemberRegistrationController : MemberRegistrationController
        {
            public IWeeeClient ApiClient { get; private set; }

            public FakeMemberRegistrationController(IWeeeClient apiClient, IFileConverterService fileConverter, IWeeeCache cache, BreadcrumbService breadcrumb)
                : base(() => apiClient, fileConverter, cache, breadcrumb)
            {
                ApiClient = apiClient;
            }

            public void InvokeOnActionExecuting(ActionExecutingContext filterContext)
            {
                OnActionExecuting(filterContext);
            }
        }

        private class ActionExecutingContextHelper
        {
            public static ActionDescriptor FakeActionDescriptorWithActionName(string actionName)
            {
                var fakeActionDescriptor = A.Fake<ActionDescriptor>();
                A.CallTo(() => fakeActionDescriptor.ActionName).Returns(actionName);

                return fakeActionDescriptor;
            }

            public static IDictionary<string, object> FakeActionParameters()
            {
                return A.Fake<IDictionary<string, object>>();
            }

            public static IDictionary<string, object> FakeActionParameters(bool retrievalResult, Guid outValue)
            {
                var fakeActionParameters = FakeActionParameters();

                object dummyObject;
                A.CallTo(() => fakeActionParameters.TryGetValue(A<string>._, out dummyObject))
                    .Returns(retrievalResult)
                    .AssignsOutAndRefParameters(outValue);

                return fakeActionParameters;
            }
        }
    }
}
