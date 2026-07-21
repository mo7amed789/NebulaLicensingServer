using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NebulaLicensingServer.Domain.Enums;
using NebulaLicensingServer.DTOs.Licenses.Requests;
using NebulaLicensingServer.DTOs.Licenses.Responses;
using NebulaLicensingServer.Interfaces;
using NebulaLicensingServer.ViewModels.Dashboard;

namespace NebulaLicensingServer.Controllers;

[Authorize(AuthenticationSchemes = CookieAuthenticationDefaults.AuthenticationScheme)]
public class DashboardController : Controller
{
    private readonly ILicenseService _licenseService;

    public DashboardController(ILicenseService licenseService)
    {
        _licenseService = licenseService;
    }

    [HttpGet]
    public async Task<IActionResult> Index(string? search, string? status, int page = 1, int pageSize = 10, CancellationToken cancellationToken = default)
    {
        var model = await BuildDashboardViewModelAsync(search, status, page, pageSize, cancellationToken);
        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> Snapshot(string? search, string? status, int page = 1, int pageSize = 10, CancellationToken cancellationToken = default)
    {
        var model = await BuildDashboardViewModelAsync(search, status, page, pageSize, cancellationToken);
        return Json(new
        {
            success = true,
            totalLicenses = model.TotalLicenses,
            activeLicenses = model.ActiveLicenses,
            expiredLicenses = model.ExpiredLicenses,
            revokedLicenses = model.RevokedLicenses,
            todayActivations = model.TodayActivations,
            todayValidations = model.TodayValidations,
            totalCount = model.TotalCount,
            licenses = model.Licenses.Select(x => new
            {
                x.Id,
                x.LicenseKey,
                x.Status,
                x.DeviceName,
                x.MachineHash,
                x.ActivatedAt,
                x.ExpiresAt,
                x.LastValidation,
                x.Notes,
                x.IsExpired
            })
        });
    }

    [HttpGet]
    public async Task<IActionResult> Details(string id, CancellationToken cancellationToken)
    {
        var result = await _licenseService.GetLicenseByKeyAsync(id, cancellationToken);
        if (result.IsFailure || result.Value is null)
        {
            return NotFound();
        }

        return View("Details", new LicenseListItemViewModel
        {
            Id = result.Value.Id,
            LicenseKey = result.Value.LicenseKey,
            Status = result.Value.Status.ToString(),
            DeviceName = result.Value.DeviceName,
            MachineHash = result.Value.MachineHash,
            ActivatedAt = result.Value.ActivatedAt,
            ExpiresAt = result.Value.ExpiresAt,
            LastValidation = result.Value.LastValidation,
            Notes = result.Value.Notes,
            IsExpired = (result.Value.ExpiresAt.HasValue && result.Value.ExpiresAt.Value <= DateTime.UtcNow) || result.Value.Status == LicenseStatus.Expired
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Generate(int months, int days, int hours, int minutes, string? notes, CancellationToken cancellationToken)
    {
        if (months < 0 || days < 0 || hours < 0 || minutes < 0)
        {
            return BadRequest(new { success = false, message = "Time values cannot be negative." });
        }

        if (months == 0 && days == 0 && hours == 0 && minutes == 0)
        {
            return BadRequest(new { success = false, message = "Please specify a valid validation duration." });
        }

        var result = await _licenseService.GenerateLicenseAsync(new GenerateLicenseRequest
        {
            ValidMonths = months,
            ValidDays = days,
            ValidHours = hours,
            ValidMinutes = minutes,
            Notes = notes
        }, cancellationToken);

        if (result.IsFailure || result.Value is null)
        {
            return BadRequest(new { success = false, message = result.Error });
        }

        return Ok(new { success = true, licenseKey = result.Value.LicenseKey });
    }

    private async Task<DashboardViewModel> BuildDashboardViewModelAsync(string? search, string? status, int page, int pageSize, CancellationToken cancellationToken)
    {
        var request = new LicenseSearchRequest
        {
            LicenseKey = search,
            Status = Enum.TryParse<LicenseStatus>(status, true, out var parsedStatus) ? parsedStatus : null,
            Page = page,
            PageSize = pageSize
        };

        var result = await _licenseService.SearchLicensesAsync(request, cancellationToken);
        var licenses = result.Value?.Items ?? [];
        var allLicenses = (await _licenseService.GetAllLicensesAsync(cancellationToken)).Value ?? [];

        return new DashboardViewModel
        {
            Search = search,
            Status = status,
            Page = result.Value?.Page ?? page,
            PageSize = result.Value?.PageSize ?? pageSize,
            TotalCount = result.Value?.TotalCount ?? 0,
            TotalLicenses = allLicenses.Count,
            ActiveLicenses = allLicenses.Count(x => x.Status == LicenseStatus.Activated),
            ExpiredLicenses = allLicenses.Count(x => x.Status == LicenseStatus.Expired),
            RevokedLicenses = allLicenses.Count(x => x.Status == LicenseStatus.Revoked),
            TodayActivations = allLicenses.Count(x => x.ActivatedAt?.Date == DateTime.UtcNow.Date),
            TodayValidations = allLicenses.Count(x => x.LastValidation?.Date == DateTime.UtcNow.Date),
            LatestLicensesCount = licenses.Count,
            Licenses = licenses.Select(x => new LicenseListItemViewModel
            {
                Id = x.Id,
                LicenseKey = x.LicenseKey,
                Status = x.Status.ToString(),
                DeviceName = x.DeviceName,
                MachineHash = x.MachineHash,
                ActivatedAt = x.ActivatedAt,
                ExpiresAt = x.ExpiresAt,
                LastValidation = x.LastValidation,
                Notes = x.Notes,
                IsExpired = (x.ExpiresAt.HasValue && x.ExpiresAt.Value <= DateTime.UtcNow) || x.Status == LicenseStatus.Expired
            }).ToList(),
            ChartLabelsJson = System.Text.Json.JsonSerializer.Serialize(new[] { "Active", "Expired", "Revoked" }),
            ChartDataJson = System.Text.Json.JsonSerializer.Serialize(new[]
            {
                allLicenses.Count(x => x.Status == LicenseStatus.Activated),
                allLicenses.Count(x => x.Status == LicenseStatus.Expired),
                allLicenses.Count(x => x.Status == LicenseStatus.Revoked)
            })
        };
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Extend(string licenseKey, int days, CancellationToken cancellationToken)
    {
        if (days <= 0)
        {
            return BadRequest(new { success = false, message = "Extension duration must be greater than zero." });
        }

        var license = (await _licenseService.GetLicenseByKeyAsync(licenseKey, cancellationToken)).Value;
        if (license is null)
        {
            return NotFound(new { success = false, message = "License not found." });
        }

        if (!license.ExpiresAt.HasValue)
        {
            return BadRequest(new { success = false, message = "Cannot extend a license that has not been activated yet." });
        }

        var result = await _licenseService.ExtendLicenseAsync(new ExtendLicenseRequest
        {
            LicenseKey = licenseKey,
            NewExpirationDate = license.ExpiresAt.Value.AddDays(days)
        }, cancellationToken);

        if (result.IsFailure || result.Value is null)
        {
            return BadRequest(new { success = false, message = result.Error });
        }

        return Ok(new
        {
            success = true,
            licenseKey = result.Value.LicenseKey,
            expiresAt = result.Value.ExpiresAt?.ToString("O")
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Revoke(string licenseKey, string? reason, CancellationToken cancellationToken)
    {
        var result = await _licenseService.RevokeLicenseAsync(new RevokeLicenseRequest
        {
            LicenseKey = licenseKey
        }, cancellationToken);

        if (result.IsFailure || result.Value is null)
        {
            return BadRequest(new { success = false, message = result.Error });
        }

        return Ok(new
        {
            success = true,
            licenseKey = result.Value.LicenseKey,
            reason = reason ?? string.Empty
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(string licenseKey, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(licenseKey))
        {
            return BadRequest(new { success = false, message = "License key is required." });
        }

        var result = await _licenseService.DeleteLicenseAsync(licenseKey, cancellationToken);

        if (result.IsFailure)
        {
            return BadRequest(new { success = false, message = result.Error });
        }

        return Ok(new { success = true, licenseKey });
    }
}
