﻿namespace EA.Weee.RequestHandlers.Shared.DomainUser
{
    using Domain.User;
    using System.Threading.Tasks;

    /// <summary>
    /// Provides methods for fetching objects representing domain users.
    /// </summary>
    public interface IDomainUserContext
    {
        /// <summary>
        /// Fetches the domain user representing the current application user.
        /// </summary>
        /// <returns></returns>
        Task<User> GetCurrentUserAsync();

        /// <summary>
        /// Fetches the domain user representing the specified application user.
        /// </summary>
        /// <param name="userId">The ID of the application user.</param>
        /// <returns></returns>
        Task<User> GetUserAsync(string userId);
    }
}
