﻿namespace EA.Weee.Email
{
    using EA.Prsd.Email;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Mail;
    using System.Text;
    using System.Threading.Tasks;

    public class WeeeEmailService : IWeeeEmailService
    {
        private readonly ITemplateExecutor templateExecutor;
        private readonly IMessageCreator messageCreator;
        private readonly ISender sender;

        public WeeeEmailService(
            ITemplateExecutor templateExecutor,
            IMessageCreator messageCreator,
            ISender sender)
        {
            this.templateExecutor = templateExecutor;
            this.messageCreator = messageCreator;
            this.sender = sender;
        }

        public async Task<bool> SendActivateUserAccount(string emailAddress, string activationUrl)
        {
            var model = new
            {
                ActivationUrl = activationUrl,
            };

            EmailContent content = new EmailContent()
            {
                HtmlText = templateExecutor.Execute("ActivateUserAccount.cshtml", model),
                PlainText = templateExecutor.Execute("ActivateUserAccount.txt", model)
            };

            MailMessage message = messageCreator.Create(
                emailAddress,
                "Activate your WEEE user account",
                content);

            return await sender.SendAsync(message);
        }
    }
}
