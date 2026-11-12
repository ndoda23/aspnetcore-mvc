using BulkyBook.Business.Services.IServices;
using BulkyBook.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BulkyBookWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.RoleAdmin)]
    public class AuditLogController : Controller
    {
        private readonly IAuditLogService _auditLogService;

        public AuditLogController(IAuditLogService auditLogService)
        {
            _auditLogService = auditLogService;
        }

        public IActionResult Index()
        {
            return View();
        }

        #region API CALLS

        public async Task<IActionResult> GetAll(string? entity, DateTime? from, DateTime? to)
        {
            if (entity == "all") entity = null;

            DateTime? toExclusive = to?.Date.AddDays(1);

            var logs = await _auditLogService.GetAllAsync(entity, from?.Date, toExclusive);

            var result = logs.Select(l => new
            {
                id = l.Id,
                timestamp = l.Timestamp,
                userEmail = l.UserEmail ?? l.UserId,
                action = l.Action,
                entityName = l.EntityName,
                entityId = l.EntityId,
                oldValue = l.OldValue,
                newValue = l.NewValue
            });

            return Json(new { data = result });
        }

        #endregion
    }
}