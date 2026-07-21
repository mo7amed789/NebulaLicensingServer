namespace NebulaLicensingServer.DTOs.Licenses.Requests;

public class GenerateLicenseRequest
{
    public int ValidMonths { get; set; }
    public int ValidDays { get; set; }
    public int ValidHours { get; set; }
    public int ValidMinutes { get; set; }
    public string? Notes { get; set; }
}
