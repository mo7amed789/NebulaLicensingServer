using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NebulaLicensingServer.DTOs.AuditLogs.Requests;
using NebulaLicensingServer.DTOs.AuditLogs.Responses;
using NebulaLicensingServer.DTOs.Licenses.Responses;
using NebulaLicensingServer.Interfaces;

namespace NebulaLicensingServer.Controllers;

[Authorize(AuthenticationSchemes = CookieAuthenticationDefaults.AuthenticationScheme)]
public sealed class AuditLogsController(IAuditLogService auditLogService) : Controller
{
    private readonly IAuditLogService _auditLogService = auditLogService;

    public async Task<IActionResult> Index(string? user, string? action, string? license, string? ipAddress, DateTime? from, DateTime? to, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default)
    {
        var result = await _auditLogService.SearchAsync(new AuditLogSearchRequest
        {
            User = user,
            Action = action,
            License = license,
            IpAddress = ipAddress,
            From = from,
            To = to,
            Page = page,
            PageSize = pageSize
        }, cancellationToken);

        return View(result.Value ?? new PagedResult<AuditLogResponse>());
    }
}
