using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NebulaLicensingServer.Constants;

namespace NebulaLicensingServer.Controllers;

[Authorize(Roles = Roles.Administrator)]
public class LicenseDashboardController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
}