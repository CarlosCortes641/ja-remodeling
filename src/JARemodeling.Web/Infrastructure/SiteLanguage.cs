namespace JARemodeling.Web.Infrastructure;

/// <summary>
/// Language comes from the URL (/es/...) so each language has its own indexable address,
/// matching the prototype's hreflang structure.
/// </summary>
public sealed class SiteLanguage
{
    public SiteLanguage(IHttpContextAccessor accessor)
    {
        var path = accessor.HttpContext?.Request.Path.Value ?? "/";
        IsSpanish = path.Equals("/es", StringComparison.OrdinalIgnoreCase)
                    || path.StartsWith("/es/", StringComparison.OrdinalIgnoreCase);
    }

    public bool IsSpanish { get; }
    public string Code => IsSpanish ? "es" : "en";
    public string HtmlLang => IsSpanish ? "es" : "en";
    public string HrefLang => IsSpanish ? "es-US" : "en-US";

    public string T(string english, string spanish) => IsSpanish ? spanish : english;
}
