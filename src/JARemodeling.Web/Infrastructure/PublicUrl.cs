namespace JARemodeling.Web.Infrastructure;

public static class PublicUrl
{
    private static string? _configuredBase;

    /// <summary>
    /// Optional production origin (config <c>Seo:SiteUrl</c>) so canonical, hreflang and sitemap
    /// URLs stay on the primary domain even when the app is reached through another host.
    /// </summary>
    public static void Configure(string? siteUrl)
    {
        _configuredBase = string.IsNullOrWhiteSpace(siteUrl) ? null : siteUrl.Trim().TrimEnd('/');
    }

    public static string Base(HttpRequest request)
    {
        if (_configuredBase is not null && !IsLocal(request))
        {
            return _configuredBase;
        }

        var host = request.Headers["X-Forwarded-Host"].FirstOrDefault()
                   ?? request.Host.Value;
        var scheme = request.Headers["X-Forwarded-Proto"].FirstOrDefault()?.Split(',')[0].Trim();
        if (string.IsNullOrWhiteSpace(scheme))
        {
            scheme = IsLocal(request) ? request.Scheme : "https";
        }

        return $"{scheme}://{host}".TrimEnd('/');
    }

    public static string Absolute(HttpRequest request, string path) =>
        path == "/" ? Base(request) + "/" : Base(request) + path;

    public static bool IsLocal(HttpRequest request)
    {
        var host = request.Host.Host;
        return string.Equals(host, "localhost", StringComparison.OrdinalIgnoreCase)
               || string.Equals(host, "127.0.0.1", StringComparison.OrdinalIgnoreCase);
    }
}
