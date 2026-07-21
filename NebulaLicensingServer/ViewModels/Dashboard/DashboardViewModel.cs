namespace NebulaLicensingServer.ViewModels.Dashboard;

public sealed class DashboardViewModel
{
    public string? Search { get; set; }

    public string? Status { get; set; }

    public int Page { get; set; } = 1;

    public int PageSize { get; set; } = 10;

    public int TotalCount { get; set; }

    public int TotalLicenses { get; set; }

    public int ActiveLicenses { get; set; }

    public int ExpiredLicenses { get; set; }

    public int RevokedLicenses { get; set; }

    public int TodayActivations { get; set; }

    public int TodayValidations { get; set; }

    public int LatestLicensesCount { get; set; }

    public string ChartLabelsJson { get; set; } = "[]";

    public string ChartDataJson { get; set; } = "[]";

    public List<LicenseListItemViewModel> Licenses { get; set; } = [];

    public string? GeneratedLicenseKey { get; set; }

    public bool GeneratedLicenseCreated { get; set; }
}
