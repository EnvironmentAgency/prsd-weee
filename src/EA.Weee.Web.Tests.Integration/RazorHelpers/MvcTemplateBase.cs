﻿namespace EA.Weee.Web.Tests.Integration.RazorHelpers
{
    using RazorEngine.Templating;
    using System.Web.Mvc;

    public class MvcTemplateBase<T> : HtmlTemplateBase<T>
    {
        public HtmlHelper<T> HtmlHelper { get; private set; }

        public MvcTemplateBase()
        {
            var customWebViewPage = new CustomWebViewPage<T>();

            HtmlHelper = customWebViewPage.Html;
        }

        public new string ResolveUrl(string path)
        {
            // Do nothing
            return string.Empty;
        }
    }
}
