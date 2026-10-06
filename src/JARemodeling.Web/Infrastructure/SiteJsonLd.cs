namespace JARemodeling.Web.Infrastructure;

public sealed class SiteJsonLd
{
    private readonly string _template;

    public SiteJsonLd(IWebHostEnvironment env)
    {
        _template = File.ReadAllText(Path.Combine(env.ContentRootPath, "Data", "business.jsonld.json"));
    }

    public string Render(string baseUrl) =>
        _template.Replace("{{BASE_URL}}", baseUrl).Replace("</", "<\\/");
}
