﻿namespace EA.Weee.Web.Areas.Admin.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Threading.Tasks;
    using System.Web.Mvc;
    using EA.Weee.Api.Client;
    using EA.Weee.Core.Admin;
    using EA.Weee.Core.Search;
    using EA.Weee.Requests.Admin;
    using EA.Weee.Web.Areas.Admin.Controllers.Base;
    using EA.Weee.Web.Areas.Admin.ViewModels.Producers;
    using EA.Weee.Web.Infrastructure;
    using EA.Weee.Web.Services;

    public class ProducersController : AdminController
    {
        private readonly BreadcrumbService breadcrumb;
        private readonly ISearcher<ProducerSearchResult> producerSearcher;
        private readonly Func<IWeeeClient> apiClient;
        private const int maximumSearchResults = 10;

        public ProducersController(BreadcrumbService breadcrumb, ISearcher<ProducerSearchResult> producerSearcher, Func<IWeeeClient> apiClient)
        {
            this.breadcrumb = breadcrumb;
            this.producerSearcher = producerSearcher;
            this.apiClient = apiClient;
        }

        /// <summary>
        /// This method is used by both JS and non-JS users.
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<ActionResult> Search()
        {
            await SetBreadcrumb();
            return View();
        }

        /// <summary>
        /// This method is used by non-JS users to retrieve search results.
        /// </summary>
        /// <param name="viewModel"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Search(SearchViewModel viewModel)
        {
            await SetBreadcrumb();

            if (!ModelState.IsValid)
            {
                return View(viewModel);
            }

            // Check to see if a registration number was selected.
            if (!string.IsNullOrEmpty(viewModel.SelectedRegistrationNumber))
            {
                return RedirectToAction("Details", new
                {
                    RegistrationNumber = viewModel.SelectedRegistrationNumber
                });
            }

            return RedirectToAction("SearchResults", new { viewModel.SearchTerm });
        }

        /// <summary>
        /// This method is used by users who are not using the auto-complete.
        /// It loads the search results page for a specified search term.
        /// </summary>
        /// <param name="viewModel"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<ActionResult> SearchResults(string searchTerm)
        {
            await SetBreadcrumb();

            SearchResultsViewModel viewModel = new SearchResultsViewModel();
            viewModel.SearchTerm = searchTerm;
            viewModel.Results = await producerSearcher.Search(searchTerm, maximumSearchResults, false);

            return View(viewModel);
        }

        /// <summary>
        /// This method is used to select an item from the search results page.
        /// </summary>
        /// <param name="viewModel"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SearchResults(SearchResultsViewModel viewModel)
        {
            await SetBreadcrumb();

            if (!ModelState.IsValid)
            {
                viewModel.Results = await producerSearcher.Search(viewModel.SearchTerm, maximumSearchResults, false);

                return View(viewModel);
            }

            return RedirectToAction("Details", new
            {
                RegistrationNumber = viewModel.SelectedRegistrationNumber
            });
        }

        /// <summary>
        /// This method is called using AJAX by JS-users.
        /// </summary>
        /// <param name="viewModel"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> FetchSearchResultsJson(string searchTerm)
        {
            if (!Request.IsAjaxRequest())
            {
                throw new InvalidOperationException();
            }

            if (!ModelState.IsValid)
            {
                return Json(null, JsonRequestBehavior.AllowGet);
            }

            IList<ProducerSearchResult> searchResults = await producerSearcher.Search(searchTerm, maximumSearchResults, true);

            return Json(searchResults, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public async Task<ActionResult> Details(string registrationNumber)
        {
            await SetBreadcrumb();

            ProducerDetails producerDetails;
            using (IWeeeClient client = apiClient())
            {
                GetProducerDetails request = new GetProducerDetails()
                {
                    RegistrationNumber = registrationNumber
                };

                producerDetails = await client.SendAsync(User.GetAccessToken(), request);
            }

            DetailsViewModel viewModel = new DetailsViewModel();
            viewModel.Details = producerDetails;

            return View(viewModel);
        }

        [HttpGet]
        public async Task<ActionResult> DownloadProducerAmendmentsCsv(string registrationNumber)
        {
            using (IWeeeClient client = apiClient())
            {
                var producerAmendmentsCsvData = await client.SendAsync(User.GetAccessToken(),
                    new GetProducerAmendmentsHistoryCSV(registrationNumber));

                byte[] data = new UTF8Encoding().GetBytes(producerAmendmentsCsvData.FileContent);
                return File(data, "text/csv", CsvFilenameFormat.FormatFileName(producerAmendmentsCsvData.FileName));
            }
        }

        [HttpGet]
        public async Task<ActionResult> ConfirmRemoval(Guid registeredProducerId)
        {
            await SetBreadcrumb();
            using (IWeeeClient client = apiClient())
            {
                ProducerDetailsScheme producer = await client.SendAsync(User.GetAccessToken(),
                    new GetProducerDetailsByRegisteredProducerId(registeredProducerId));

                return View(new ConfirmRemovalViewModel
                {
                    RegisteredProducerId = registeredProducerId,
                    RegistrationNumber = producer.Prn,
                    ComplianceYear = producer.ComplianceYear,
                    ProducerName = producer.ProducerName,
                    SchemeName = producer.SchemeName
                });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ConfirmRemoval(ConfirmRemovalViewModel model)
        {
            await SetBreadcrumb();

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            if (model.SelectedValue == "No")
            {
                return RedirectToAction("Details", new { model.RegistrationNumber });
            }
            if (model.SelectedValue == "Yes")
            {
                using (var client = apiClient())
                {
                    await client.SendAsync(User.GetAccessToken(), new RemoveProducer(model.RegisteredProducerId));

                    return RedirectToAction("Removed",
                            new { model.RegistrationNumber, model.ComplianceYear, model.SchemeName });
                }
            }
            return View(model);
        }

        [HttpGet]
        public async Task<ActionResult> Removed(string registrationNumber, int complianceYear, string schemeName)
        {
            await SetBreadcrumb();
            return View(new RemovedViewModel
            {
                RegistrationNumber = registrationNumber,
                ComplianceYear = complianceYear,
                SchemeName = schemeName
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Removed(RemovedViewModel model)
        {
            using (IWeeeClient client = apiClient())
            {
                var isAssociate = await client.SendAsync(User.GetAccessToken(),
                    new IsProducerAssociatedWithAnotherScheme(model.RegistrationNumber, model.ComplianceYear));

                if (isAssociate)
                {
                    return RedirectToAction("Details", new { model.RegistrationNumber });
                }
                return RedirectToAction("Search");
            }
        }

        private async Task SetBreadcrumb()
        {
            breadcrumb.InternalActivity = "Producer details";

            await Task.Yield();
        }
    }
}