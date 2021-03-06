﻿namespace EA.Weee.Web.Services
{
    using EA.Weee.Core.Routing;
    using System.Web;
    using System.Web.Mvc;

    /// <summary>
    /// Uses the current HttpContext to resolve routes that are used externally
    /// to the WEEE website.
    /// </summary>
    public class ExternalRouteService : IExternalRouteService
    {
        private UrlHelper UrlHelper
        {
            get
            {
                return new UrlHelper(HttpContext.Current.Request.RequestContext);
            }
        }

        public string ActivateInternalUserAccountUrl
        {
            get
            {
                string protocol = UrlHelper.RequestContext.HttpContext.Request.Url.Scheme;
                return UrlHelper.Action("ActivateUserAccount", "Account", new { area = "Admin" }, protocol);
            }
        }

        public string ActivateExternalUserAccountUrl
        {
            get
            {
                string protocol = UrlHelper.RequestContext.HttpContext.Request.Url.Scheme;
                return UrlHelper.Action("ActivateUserAccount", "Account", null, protocol);
            }
        }

        public ResetPasswordRoute InternalUserResetPasswordRoute
        {
            get
            {
                string protocol = UrlHelper.RequestContext.HttpContext.Request.Url.Scheme;

                var routeValues = new
                {
                    area = "admin",
                    id = ResetPasswordRoute.PlaceholderUserId,
                    token = ResetPasswordRoute.PlaceholderToken,
                };

                string url = UrlHelper.Action("ResetPassword", "Account", routeValues, protocol);

                return new ResetPasswordRoute(url);
            }
        }

        public ResetPasswordRoute ExternalUserResetPasswordRoute
        {
            get
            {
                string protocol = UrlHelper.RequestContext.HttpContext.Request.Url.Scheme;

                var routeValues = new
                {
                    id = ResetPasswordRoute.PlaceholderUserId,
                    token = ResetPasswordRoute.PlaceholderToken,
                };

                string url = UrlHelper.Action("ResetPassword", "Account", routeValues, protocol);

                return new ResetPasswordRoute(url);
            }
        }

        public ViewCompetentAuthorityUserRoute ViewCompetentAuthorityUserRoute
        {
            get
            {
                string protocol = UrlHelper.RequestContext.HttpContext.Request.Url.Scheme;

                var routeValues = new
                {
                    area = "admin",
                    id = ViewCompetentAuthorityUserRoute.PlaceholderUserId
                };

                string url = UrlHelper.Action("View", "User", routeValues, protocol);

                return new ViewCompetentAuthorityUserRoute(url);
            }
        }
    }
}