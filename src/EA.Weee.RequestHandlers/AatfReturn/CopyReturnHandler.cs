namespace EA.Weee.RequestHandlers.AatfReturn
{
    using DataAccess;
    using Domain.AatfReturn;
    using Factories;
    using Prsd.Core.Domain;
    using Prsd.Core.Mediator;
    using Requests.AatfReturn;
    using Security;
    using System;
    using System.Data.Entity;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Threading.Tasks;
    using QuarterWindow = Domain.DataReturns.QuarterWindow;

    internal class CopyReturnHandler : IRequestHandler<CopyReturn, Guid>
    {
        private readonly IWeeeAuthorization authorization;
        private readonly IUserContext userContext;
        private readonly WeeeContext context;
        private readonly IQuarterWindowFactory quarterWindowFactory;

        public CopyReturnHandler(IWeeeAuthorization authorization,
            WeeeContext context,
            IUserContext userContext,
            IQuarterWindowFactory quarterWindowFactory)
        {
            this.authorization = authorization;
            this.userContext = userContext;
            this.quarterWindowFactory = quarterWindowFactory;
            this.context = context;
        }

        public async Task<Guid> HandleAsync(CopyReturn message)
        {
            authorization.EnsureCanAccessExternalArea();

            var returnCopy = await context.Returns.Include(r => r.Organisation).FirstOrDefaultAsync(r => r.Id == message.ReturnId);

            if (returnCopy == null)
            {
                throw new ArgumentException($"No return was found with id {message.ReturnId}.");
            }

            authorization.EnsureOrganisationAccess(returnCopy.Organisation.Id);

            using (var transaction = context.Database.BeginTransaction())
            {
                try
                {
                    await CopyReturnReportsOn(message, returnCopy);

                    await CopyReturnSchemes(message, returnCopy);

                    await CopyReturnNonObligated(message, returnCopy);

                    CopyReturnReceived(message, returnCopy);

                    CopyReturnSentOn(message, returnCopy);

                    CopyReturnReused(message, returnCopy);

                    CopyReturn(message, returnCopy);

                    await context.SaveChangesAsync();

                    await RemoveAatfsWithInvalidApprovalDate(returnCopy);

                    await context.SaveChangesAsync();

                    await RemoveAnyDuplicateNonObligatedAndSave(returnCopy);

                    transaction.Commit();

                    return @returnCopy.Id;
                }
                catch
                {
                    transaction.Rollback();

                    throw;
                }
                finally
                {
                    transaction.Dispose();
                }
            }
        }

        private async Task RemoveAatfsWithInvalidApprovalDate(Return returnCopy)
        {
            var quarterWindow = await quarterWindowFactory.GetQuarterWindow(returnCopy.Quarter);

            var sentOnToRemove = context.WeeeSentOn.Where(Predicate<WeeeSentOn>(quarterWindow, returnCopy))
                .Include(w => w.WeeeSentOnAmounts).ToList();

            context.WeeeSentOnAmount.RemoveRange(sentOnToRemove.SelectMany(r => r.WeeeSentOnAmounts));
            context.WeeeSentOn.RemoveRange(sentOnToRemove);

            var reusedToRemove = context.WeeeReused.Where(Predicate<WeeeReused>(quarterWindow, returnCopy))
                .Include(w => w.WeeeReusedAmounts)
                .Include(w => w.WeeeReusedSites).ToList();

            context.WeeeReusedAmount.RemoveRange(reusedToRemove.SelectMany(r => r.WeeeReusedAmounts));
            context.WeeeReusedSite.RemoveRange(reusedToRemove.SelectMany(r => r.WeeeReusedSites));
            context.WeeeReused.RemoveRange(reusedToRemove);

            var receivedToRemove = context.WeeeReceived.Where(Predicate<WeeeReceived>(quarterWindow, returnCopy))
                .Include(w => w.WeeeReceivedAmounts).ToList();

            context.WeeeReceivedAmount.RemoveRange(receivedToRemove.SelectMany(r => r.WeeeReceivedAmounts));
            context.WeeeReceived.RemoveRange(receivedToRemove);
        }

        private async Task RemoveAnyDuplicateNonObligatedAndSave(Return returnCopy)
        {
            var nonObligatedForReport = await context.NonObligatedWeee
                .Where(n => n.ReturnId == returnCopy.Id)
                .ToListAsync();

            var nonObligatedToRemove = nonObligatedForReport
                .GroupBy(n => n.CategoryId)
                .Where(g => g.Count() > 1)
                .SelectMany(g => g.Skip(1));

            if (nonObligatedToRemove.Count() > 0)
            {
                context.NonObligatedWeee.RemoveRange(nonObligatedToRemove);
                await context.SaveChangesAsync();
            }
        }

        private static Expression<Func<T, bool>> Predicate<T>(QuarterWindow quarterWindow, Return returnCopy) where T : AatfEntity
        {
            return w => (w.Aatf.ApprovalDate == null || w.Aatf.ApprovalDate.Value >= quarterWindow.StartDate) && w.ReturnId == returnCopy.Id;
        }

        private void CopyReturn(CopyReturn message, Return returnCopy)
        {
            returnCopy.ResetSubmitted(userContext.UserId.ToString(), message.ReturnId);
            context.Entry(returnCopy.Organisation).State = EntityState.Unchanged;

            context.Returns.Add(returnCopy);
        }

        private void CopyReturnReused(CopyReturn message, Return returnCopy)
        {
            var reused = context.WeeeReused.Where(w => w.ReturnId == message.ReturnId)
                .Include(w => w.WeeeReusedSites.Select(s => s.Address))
                .Include(w => w.WeeeReusedAmounts)
                .Include(w => w.Aatf)
                .ToList();

            if (reused.Any())
            {
                reused.ForEach(s => s.UpdateReturn(returnCopy));
                reused.ForEach(r => context.Entry(r.Aatf).State = EntityState.Unchanged);
                reused.ForEach(r => r.WeeeReusedSites.ToList().ForEach(w => context.Entry(w.Address).State = EntityState.Added));
                reused.ForEach(r => r.WeeeReusedSites.ToList().ForEach(w => context.Entry(w).State = EntityState.Added));
                reused.ForEach(r => r.WeeeReusedAmounts.ToList().ForEach(w => context.Entry(w).State = EntityState.Added));

                context.WeeeReused.AddRange(reused);
            }
        }

        private void CopyReturnSentOn(CopyReturn message, Return returnCopy)
        {
            var sentOn = context.WeeeSentOn.Where(w => w.ReturnId == message.ReturnId)
                .Include(w => w.WeeeSentOnAmounts)
                .Include(w => w.OperatorAddress)
                .Include(w => w.Aatf)
                .Include(w => w.SiteAddress)
                .ToList();

            if (sentOn.Any())
            {
                sentOn.ForEach(s => s.UpdateReturn(returnCopy));
                sentOn.ForEach(s => s.WeeeSentOnAmounts.ToList().ForEach(w => context.Entry(w).State = EntityState.Added));

                foreach (var weeeSentOn in sentOn)
                {
                    context.Entry(weeeSentOn.SiteAddress).State = EntityState.Added;
                    context.Entry(weeeSentOn.Aatf).State = EntityState.Unchanged;

                    if (weeeSentOn.OperatorAddress != null)
                    {
                        context.Entry(weeeSentOn.OperatorAddress).State = EntityState.Added;
                    }
                }

                context.WeeeSentOn.AddRange(sentOn);
            }
        }

        private void CopyReturnReceived(CopyReturn message, Return returnCopy)
        {
            var received = context.WeeeReceived.Where(w => w.ReturnId == message.ReturnId)
                .Include(w => w.WeeeReceivedAmounts)
                .ToList();

            if (received.Any())
            {
                received.ForEach(r => r.UpdateReturn(returnCopy));
                received.ForEach(r => r.WeeeReceivedAmounts.ToList().ForEach(w => context.Entry(w).State = EntityState.Added));
                received.ForEach(s => context.Entry(s.Aatf).State = EntityState.Unchanged);
                received.ForEach(s => context.Entry(s.Scheme).State = EntityState.Unchanged);

                context.WeeeReceived.AddRange(received);
            }
        }

        private async Task CopyReturnNonObligated(CopyReturn message, Return returnCopy)
        {
            var nonObligated = await context.NonObligatedWeee
                .Where(n => n.ReturnId == message.ReturnId)
                .ToListAsync();

            nonObligated.ForEach(n => n.UpdateReturn(returnCopy));

            context.NonObligatedWeee.AddRange(nonObligated);
        }

        private async Task CopyReturnSchemes(CopyReturn message, Return returnCopy)
        {
            var schemes = await context.ReturnScheme.Where(r => r.ReturnId == message.ReturnId).ToListAsync();
            schemes.ForEach(s => s.UpdateReturn(returnCopy));

            context.ReturnScheme.AddRange(schemes);
        }

        private async Task CopyReturnReportsOn(CopyReturn message, Return returnCopy)
        {
            var reportsOn = await context.ReturnReportOns.Where(r => r.ReturnId == message.ReturnId).ToListAsync();
            reportsOn.ForEach(s => s.UpdateReturn(returnCopy));

            context.ReturnReportOns.AddRange(reportsOn);
        }
    }
}