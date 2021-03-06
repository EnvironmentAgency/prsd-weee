﻿namespace EA.Weee.Api.Identity
{
    using IdentityServer3.Core.Events;
    using IdentityServer3.Core.Services;
    using System.Threading.Tasks;

    /// <summary>
    /// An implmentation of Identity Servers's IEventService interface which routes
    /// login success and failure events to an ISecurityEventAuditor.
    /// </summary>
    public class SecurityEventService : IEventService
    {
        private readonly ISecurityEventAuditor auditSecurityEventService;

        public SecurityEventService(ISecurityEventAuditor auditSecurityEventService)
        {
            this.auditSecurityEventService = auditSecurityEventService;
        }

        public Task RaiseAsync<T>(Event<T> evt)
        {
            Event<LocalLoginDetails> localLoginDetailsEvent = evt as Event<LocalLoginDetails>;
            if (localLoginDetailsEvent != null)
            {
                AuditLogin(localLoginDetailsEvent);
            }

            return Task.FromResult(0);
        }

        private void AuditLogin(Event<LocalLoginDetails> localLoginDetailsEvent)
        {
            bool success = localLoginDetailsEvent.EventType == EventTypes.Success;

            if (success)
            {
                string userId = localLoginDetailsEvent.Details.SubjectId;
                auditSecurityEventService.LoginSuccess(userId);
            }
            else
            {
                string userName = localLoginDetailsEvent.Details.LoginUserName;
                auditSecurityEventService.LoginFailure(userName);
            }
        }
    }
}