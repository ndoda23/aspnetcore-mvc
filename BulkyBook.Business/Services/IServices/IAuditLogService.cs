using BulkyBook.Models;
using System.Security.Claims;

namespace BulkyBook.Business.Services.IServices
{
    public interface IAuditLogService
    {
        Task LogAsync(ClaimsPrincipal user, string action, string entityName,
                      string? entityId = null, string? oldValue = null, string? newValue = null);

        Task<IEnumerable<AuditLog>> GetAllAsync(string? entityName = null, DateTime? from = null, DateTime? to = null);

        Task<IEnumerable<AuditLog>> GetByEntityAsync(string entityName, string entityId);
    }
}