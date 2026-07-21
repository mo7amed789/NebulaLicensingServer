using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using NebulaLicensingServer.DTOs.Requests;
using NebulaLicensingServer.Interfaces;
using NebulaLicensingServer.ViewModels;

namespace NebulaLicensingServer.Controllers;

public class DashboardAuthController : Controller
{
    private readonly IAuthService _authService;
    private readonly IAuditLogService _auditLogService;

    public DashboardAuthController(IAuthService authService, IAuditLogService auditLogService)
    {
        _authService = authService;
        _auditLogService = auditLogService;
    }

    [HttpGet("/Login")]
    public IActionResult Login()
    {
        return View(new LoginViewModel());
    }

    [HttpPost("/Login")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var result = await _authService.AuthenticateAsync(new LoginRequest
        {
            Username = model.Username,
            Password = model.Password
        });

        if (result.IsFailure)
        {
            model.Error = "Invalid username or password.";
            return View(model);
        }

        var principal = new ClaimsPrincipal(
            new ClaimsIdentity(
            [
                new Claim(ClaimTypes.Name, model.Username),
                new Claim(ClaimTypes.Role, "Administrator")
            ],
            CookieAuthenticationDefaults.AuthenticationScheme));

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            principal);

        await _auditLogService.LogAsync("Administrator Login", model.Username, null, "Success", HttpContext.Connection.RemoteIpAddress?.ToString(), cancellationToken: HttpContext.RequestAborted);

        return RedirectToAction("Index", "Dashboard");
    }

    [HttpPost("/Logout")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(
            CookieAuthenticationDefaults.AuthenticationScheme);

        await _auditLogService.LogAsync("Administrator Logout", User.Identity?.Name, null, "Success", HttpContext.Connection.RemoteIpAddress?.ToString(), cancellationToken: HttpContext.RequestAborted);

        return RedirectToAction(nameof(Login));
    }
}
