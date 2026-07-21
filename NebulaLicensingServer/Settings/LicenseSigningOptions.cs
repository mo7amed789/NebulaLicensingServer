namespace NebulaLicensingServer.Settings;

public sealed class LicenseSigningOptions
{
    public const string SectionName = "-----BEGIN PUBLIC KEY-----\r\nMIICIjANBgkqhkiG9w0BAQEFAAOCAg8AMIICCgKCAgEAy8Rp7uK7U+gFkL8CL/Ji\r\nXLIsq09BkjA68gxLfwyaqRUAcIu0eZPiK4v3v+UWGcMO0ZiNaLni8Cb0vHXdoJ5+\r\n/qE+A7gvK79Iwe0VFl7xKRQytqhPb+QB76kgIJdOOtU9CX+zGKTPZ4BnxFt+xKP/\r\n7DS34FlmC8FbZBK1WaDw0YPBVN2l5scyDbpyujKy3XbEP2l8BXSGLT0ZBgQ5v+qX\r\ni6JBygg9I2DxJmBxZIodYU1fiKLcmms27foOr9FbZBskfJBliwh+XPbgAQKpssjM\r\npsrwaaN5YXPLl6pHNG74kDYmvfx2LsNmlR3CXdjHgflXtPEQ8862vJyGbQDmEd4u\r\nuSSSF25i0X/i5ElRSBVt2MvfjaZ925okwl4r18S1/09/5EhUUmq7PmEnEr2tD6bZ\r\nD9EB/xM+9RQhexjkPqgYqDS5CsBrI6HEJyKbAUpqOm3Wp6LhbOsdb0SjnmzuvB4O\r\nvixYRSw2Unxqbo/7X4npWIHuJDF7z11T4DkJr3GyYPutKbmmse1L9R4mTnHUgsnl\r\nk4ZBXmPNk3/JWx7ePtK7EZoJcXRX6nYN5pGmGXQ40B6q7gG45C0eRTTH4BRajuiO\r\n+/XbozA/znxq7VxDD+mkYfy6cR3iOAxzmC4dkioevBe1ULBFUuxuuM5HYlYbP34x\r\n1zdENeLh0YcpOH9ffQWVGCkCAwEAAQ==\r\n-----END PUBLIC KEY-----";

    public string PrivateKeyPem { get; set; } = string.Empty;

    public int OfflineGraceDays { get; set; } = 7;
}