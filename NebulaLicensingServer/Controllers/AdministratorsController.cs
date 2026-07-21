using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NebulaLicensingServer.Common;
using NebulaLicensingServer.DTOs.Administrators.Requests;
using NebulaLicensingServer.Interfaces;

namespace NebulaLicensingServer.Controllers;

[Authorize(AuthenticationSchemes = CookieAuthenticationDefaults.AuthenticationScheme)]
public sealed class AdministratorsController(IAdministratorService administratorService) : Controller
{
    private readonly IAdministratorService _administratorService = administratorService;

    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var result = await _administratorService.GetAllAsync(cancellationToken);
        return View(result.Value ?? []);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(string username, string password, string role, CancellationToken cancellationToken)
        => Json(await _administratorService.CreateAsync(new AdministratorCreateRequest { Username = username, Password = password, Role = role }, cancellationToken));

    [HttpPost]
    [ValidateAntiForgeryToken]
    public Task<IActionResult> ResetPassword(Guid id, string password, CancellationToken cancellationToken)
        => JsonResultOf(_administratorService.ResetPasswordAsync(id, password, cancellationToken));

    [HttpPost]
    [ValidateAntiForgeryToken]
    public Task<IActionResult> Disable(Guid id, bool isDisabled, CancellationToken cancellationToken)
        => JsonResultOf(_administratorService.SetDisabledAsync(id, isDisabled, cancellationToken));

    [HttpPost]
    [ValidateAntiForgeryToken]
    public Task<IActionResult> ChangeRole(Guid id, string role, CancellationToken cancellationToken)
        => JsonResultOf(_administratorService.ChangeRoleAsync(id, role, cancellationToken));

    [HttpPost]
    [ValidateAntiForgeryToken]
    public Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
        => JsonResultOf(_administratorService.DeleteAsync(id, cancellationToken));

    private async Task<IActionResult> JsonResultOf<T>(Task<Result<T>> task)
    {
        var result = await task;
        return Json(new { success = result.IsSuccess, message = result.Error });
    }
}
