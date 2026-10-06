using System.Text.Json;
using System.Text.Json.Nodes;

namespace JARemodeling.Web.Infrastructure;

public sealed class SiteJsonLd
{
    public const string OgImagePath = "/img/og-image.png";
    public const string LogoPath = "/img/logo.png";

    private readonly string _businessTemplate;
    private readonly FaqCatalog _faq;

    public SiteJsonLd(IWebHostEnvironment env, FaqCatalog faq)
    {
        _businessTemplate = File.ReadAllText(Path.Combine(env.ContentRootPath, "Data", "business.jsonld.json"));
        _faq = faq;
    }

    public string Render(HttpRequest request, SitePage page, bool spanish)
    {
        var baseUrl = PublicUrl.Base(request);
        var home = PublicUrl.Absolute(request, spanish ? SitePages.Home.SpanishPath : SitePages.Home.EnglishPath);
        var pageUrl = PublicUrl.Absolute(request, page.PathFor(spanish));
        var businessId = baseUrl + "/#business";
        var websiteId = baseUrl + "/#website";
        var language = spanish ? "es-US" : "en-US";

        var graph = new JsonArray
        {
            Business(baseUrl),
            new JsonObject
            {
                ["@type"] = "WebSite",
                ["@id"] = websiteId,
                ["url"] = baseUrl + "/",
                ["name"] = SiteInfo.LegalName,
                ["alternateName"] = SiteInfo.ShortName,
                ["inLanguage"] = new JsonArray("en-US", "es-US"),
                ["publisher"] = Ref(businessId)
            }
        };

        var webPage = new JsonObject
        {
            ["@type"] = "WebPage",
            ["@id"] = pageUrl + "#webpage",
            ["url"] = pageUrl,
            ["name"] = page.Title.For(spanish),
            ["description"] = page.Description.For(spanish),
            ["inLanguage"] = language,
            ["isPartOf"] = Ref(websiteId),
            ["about"] = Ref(businessId),
            ["primaryImageOfPage"] = baseUrl + OgImagePath
        };
        graph.Add(webPage);

        if (page.IsAudience)
        {
            webPage["breadcrumb"] = Ref(pageUrl + "#breadcrumb");
            graph.Add(new JsonObject
            {
                ["@type"] = "BreadcrumbList",
                ["@id"] = pageUrl + "#breadcrumb",
                ["itemListElement"] = new JsonArray
                {
                    Crumb(1, SiteInfo.ShortName, home),
                    Crumb(2, page.Name.For(spanish), pageUrl)
                }
            });
            graph.Add(new JsonObject
            {
                ["@type"] = "Service",
                ["@id"] = pageUrl + "#service",
                ["name"] = spanish
                    ? $"Unit turn / make-ready para {page.Name.Es.ToLowerInvariant()}"
                    : $"Unit turn / make-ready for {page.Name.En.ToLowerInvariant()}",
                ["serviceType"] = "Unit Turn / Make-Ready",
                ["description"] = page.Description.For(spanish),
                ["url"] = pageUrl,
                ["provider"] = Ref(businessId),
                ["audience"] = new JsonObject { ["@type"] = "Audience", ["audienceType"] = page.AudienceType },
                ["areaServed"] = new JsonObject
                {
                    ["@type"] = "City",
                    ["name"] = "Charlotte",
                    ["containedInPlace"] = new JsonObject { ["@type"] = "State", ["name"] = "North Carolina" }
                },
                ["availableLanguage"] = new JsonArray("English", "Spanish")
            });
        }
        else
        {
            var questions = new JsonArray();
            foreach (var item in _faq.Items)
            {
                questions.Add(new JsonObject
                {
                    ["@type"] = "Question",
                    ["name"] = item.Question.For(spanish),
                    ["acceptedAnswer"] = new JsonObject
                    {
                        ["@type"] = "Answer",
                        ["text"] = item.Answer.For(spanish)
                    }
                });
            }

            graph.Add(new JsonObject
            {
                ["@type"] = "FAQPage",
                ["@id"] = pageUrl + "#faq",
                ["inLanguage"] = language,
                ["isPartOf"] = Ref(pageUrl + "#webpage"),
                ["mainEntity"] = questions
            });
        }

        var root = new JsonObject { ["@context"] = "https://schema.org", ["@graph"] = graph };
        return root.ToJsonString(new JsonSerializerOptions
        {
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        }).Replace("</", "<\\/");
    }

    private JsonNode Business(string baseUrl)
    {
        var business = JsonNode.Parse(_businessTemplate.Replace("{{BASE_URL}}", baseUrl))!.AsObject();
        business.Remove("@context");
        business["@id"] = baseUrl + "/#business";
        business["url"] = baseUrl + "/";
        business["email"] = SiteInfo.Email;
        business["logo"] = baseUrl + LogoPath;
        business["image"] = baseUrl + OgImagePath;
        business["hasMap"] = SiteInfo.MapUrl;
        return business;
    }

    private static JsonObject Ref(string id) => new() { ["@id"] = id };

    private static JsonObject Crumb(int position, string name, string url) => new()
    {
        ["@type"] = "ListItem",
        ["position"] = position,
        ["name"] = name,
        ["item"] = url
    };
}
