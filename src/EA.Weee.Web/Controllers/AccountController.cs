﻿namespace EA.Weee.Web.Controllers
{
    using Api.Client;
    using Api.Client.Entities;
    using Authorization;
    using EA.Weee.Core.Routing;
    using EA.Weee.Web.Controllers.Base;
    using Infrastructure;
    using Microsoft.Owin.Security;
    using Prsd.Core.Web.ApiClient;
    using Prsd.Core.Web.Mvc.Extensions;
    using Prsd.Core.Web.OAuth;
    using Prsd.Core.Web.OpenId;
    using Services;
    using System;
    using System.Threading.Tasks;
    using System.Web.Mvc;
    using ViewModels.Account;

    [Authorize]
    public class AccountController : ExternalSiteController
    {
        private readonly Func<IWeeeClient> apiClient;
        private readonly IWeeeAuthorization weeeAuthorization;
        private readonly IExternalRouteService externalRouteService;

        public AccountController(Func<IWeeeClient> apiClient,
            IWeeeAuthorization weeeAuthorization,
            IExternalRouteService externalRouteService)
        {
            this.apiClient = apiClient;
            this.weeeAuthorization = weeeAuthorization;
            this.externalRouteService = externalRouteService;
        }

        [HttpGet]
        [AllowAnonymous]
        public ActionResult SignIn(string returnUrl)
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToLocal(returnUrl);
            }

            return View(new LoginViewModel
            {
                ReturnUrl = returnUrl
            });
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SignIn(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                var loginResult = await weeeAuthorization.SignIn(LoginType.External, model.Email, model.Password, model.RememberMe);

                if (loginResult.Successful)
                {
                    return RedirectToLocal(model.ReturnUrl);
                }

                ModelState.AddModelError(string.Empty, loginResult.ErrorMessage);
            }

            return View(model);
        }

        // POST: /Account/SignOut
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SignOut()
        {
            weeeAuthorization.SignOut();

            return RedirectToAction("SignIn");
        }

        private ActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return RedirectToAction("RedirectProcess");
        }

        [HttpGet]
        public ActionResult RedirectProcess()
        {
            return RedirectToRoute("SelectOrganisation");
        }

        [HttpGet]
        public ActionResult UserAccountActivationRequired()
        {
            string email = User.GetEmailAddress();

            ViewBag.UserEmailAddress = User.GetEmailAddress();

            return View("AccountActivationRequired");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> UserAccountActivationRequired(FormCollection model)
        {
            using (var client = apiClient())
            {
                string accessToken = User.GetAccessToken();

                string activationBaseUrl = externalRouteService.ActivateExternalUserAccountUrl;

                await client.User.ResendActivationEmail(accessToken, activationBaseUrl);
            }

            return View("AccountActivationRequested");
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> UserAccountActivationFailed(Guid id, AccountActivationRequestViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View("AccountActivationFailed", model);
            }

            using (var client = apiClient())
            {
                string activationBaseUrl = externalRouteService.ActivateExternalUserAccountUrl;

                var result = await client.User.ResendActivationEmailByUserId(id.ToString(), model.Email, activationBaseUrl);

                if (!result)
                {
                    ModelState.AddModelError("Email", "The email address does not match the address for your account.");
                    return View("AccountActivationFailed", model);
                }
                else
                {
                    ViewBag.Email = model.Email;
                    return View("AccountActivationRequested");
                }
            }
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult> ActivateUserAccount(Guid id, string code)
        {
            using (var client = apiClient())
            {
                bool result = await client.User.ActivateUserAccountEmailAsync(new ActivatedUserAccountData { Id = id, Code = code });

                if (!result)
                {
                    return View("AccountActivationFailed");
                }
                else
                {
                    return View("AccountActivated");
                }
            }
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult> ResetPassword(Guid id, string token)
        {
            using (var client = apiClient())
            {
                var passwordResetData = new PasswordResetData
                {
                    Password = string.Empty,
                    Token = token,
                    UserId = id
                };

                bool result = await client.User.IsPasswordResetTokenValidAsync(passwordResetData);

                if (!result)
                {
                    return View("ResetPasswordExpired");
                }
                else
                {
                    return View("ResetPassword");
                }
            }
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ResetPassword(Guid id, string token, ResetPasswordModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            using (var client = apiClient())
            {
                var passwordResetData = new PasswordResetData
                {
                    Password = model.Password,
                    Token = token,
                    UserId = id
                };

                bool result;
                try
                {
                    result = await client.User.ResetPasswordAsync(passwordResetData);
                }
                catch (ApiBadRequestException ex)
                {
                    this.HandleBadRequest(ex);

                    if (!ModelState.IsValid)
                    {
                        return View(model);
                    }
                    else
                    {
                        throw;
                    }
                }

                if (!result)
                {
                    return View("ResetPasswordExpired");
                }
                else
                {
                    return View("ResetPasswordComplete");
                }
            }
        }

        [HttpGet]
        [AllowAnonymous]
        public ActionResult ResetPasswordRequest()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ResetPasswordRequest(ResetPasswordRequestViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            using (var client = apiClient())
            {
                ResetPasswordRoute route = externalRouteService.ExternalUserResetPasswordRoute;
                PasswordResetRequest apiModel = new PasswordResetRequest(model.Email, route);

                var result = await client.User.ResetPasswordRequestAsync(apiModel);

                if (!result.ValidEmail)               
                {
                    ModelState.AddModelError("Email", "Email address not recognised.");
                    return View(model);
                }
                else
                {
                    ViewBag.Email = model.Email;
                    return View("ResetPasswordInstruction");
                }
            }
        }
    }
}