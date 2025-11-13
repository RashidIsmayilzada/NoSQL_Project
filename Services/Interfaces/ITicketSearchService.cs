using NoSQL_Project.Models;

namespace NoSQL_Project.Services.Interfaces
{
    public interface ITicketSearchService
    {

        /// <summary>
        /// Full-text-like search over tickets (Title, Description) supporting AND/OR.
        /// Results are sorted by newest first (ObjectId desc).
        /// </summary>
        /// <param name="query">Query string; supports 'AND' and 'OR' (case-insensitive).</param>
        /// <param name="myScopeOnly">
        /// If true:
        ///  - For ServiceDesk: only tickets assigned/handled by the current user.
        ///  - For normal users: only tickets reported by the current user.
        /// If false: no scope filter (used for ServiceDesk "All").
        /// </param>
        /// <param name="userId">Current user id (ClaimTypes.NameIdentifier).</param>
        /// <param name="isServiceDesk">Whether current user is ServiceDesk.</param>
        Task<IReadOnlyList<Ticket>> SearchAsync(string query, bool myScopeOnly, string userId, bool isServiceDesk);

    }

}



