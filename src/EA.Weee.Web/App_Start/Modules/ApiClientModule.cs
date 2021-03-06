﻿namespace EA.Weee.Web.Modules
{
    using Api.Client;
    using Autofac;
    using Prsd.Core.Web.OAuth;
    using Prsd.Core.Web.OpenId;
    using Services;
    using System;

    public class ApiClientModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.Register(c =>
            {
                var cc = c.Resolve<IComponentContext>();
                var config = cc.Resolve<IAppConfiguration>();
                TimeSpan timeout = TimeSpan.FromSeconds(config.ApiTimeoutInSeconds);
                return new WeeeClient(config.ApiUrl, timeout);
            }).As<IWeeeClient>();

            builder.Register(c =>
            {
                var cc = c.Resolve<IComponentContext>();
                var config = cc.Resolve<IAppConfiguration>();
                return new OAuthClient(config.ApiUrl, config.ApiClientId, config.ApiSecret);
            }).As<IOAuthClient>();

            builder.Register(c =>
            {
                var cc = c.Resolve<IComponentContext>();
                var config = cc.Resolve<IAppConfiguration>();
                return new OAuthClientCredentialClient(config.ApiUrl, config.ApiClientCredentialId, config.ApiClientCredentialSecret);
            }).As<IOAuthClientCredentialClient>();

            builder.Register(c =>
            {
                var cc = c.Resolve<IComponentContext>();
                var config = cc.Resolve<IAppConfiguration>();
                return new UserInfoClient(config.ApiUrl);
            }).As<IUserInfoClient>();
        }
    }
}