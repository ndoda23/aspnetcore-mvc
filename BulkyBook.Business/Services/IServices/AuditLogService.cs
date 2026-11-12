using BulkyBook.Business.Services.IServices;
using BulkyBook.Models;
using BulkyBookWeb.Data;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace BulkyBook.Business.Services
{
    public class AuditLogService : IAuditLogService
    {
        private readonly ApplicationDbContext _db;

        public AuditLogService(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task LogAsync(ClaimsPrincipal user, string action, string entityName,
                                   string? entityId = null, string? oldValue = null, string? newValue = null)
        {
            var log = new AuditLog
            {
                UserId = user.FindFirstValue(ClaimTypes.NameIdentifier) ?? "system",
                UserEmail = user.FindFirstValue(ClaimTypes.Email),
                Action = action,
                EntityName = entityName,
                EntityId = entityId,
                OldValue = oldValue,
                NewValue = newValue,
                Timestamp = DateTime.UtcNow
            };

            _db.AuditLogs.Add(log);
            await _db.SaveChangesAsync();
        }

        public async Task<IEnumerable<AuditLog>> GetAllAsync(string? entityName = null, DateTime? from = null, DateTime? to = null)
        {
            var query = _db.AuditLogs.AsQueryable();

            if (!string.IsNullOrEmpty(entityName))
                query = query.Where(a => a.EntityName == entityName);

            if (from.HasValue)
                query = query.Where(a => a.Timestamp >= from.Value);

            if (to.HasValue)
                query = query.Where(a => a.Timestamp < to.Value);

            return await query
                .OrderByDescending(a => a.Timestamp)
                .Take(1000)
                .ToListAsync();
        }

        public async Task<IEnumerable<AuditLog>> GetByEntityAsync(string entityName, string entityId)
        {
            return await _db.AuditLogs
                .Where(a => a.EntityName == entityName && a.EntityId == entityId)
                .OrderByDescending(a => a.Timestamp)
                .ToListAsync();
        }
    }
}