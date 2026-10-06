using System.Security;
using System.Text;
using JARemodeling.Web.Infrastructure;
using Microsoft.AspNetCore.Mvc;

namespace JARemodeling.Web.Controllers;

public class SeoController : Controller
{
    private static readonly string LastModified =
        System.IO.File.GetLastWriteTimeUtc(typeof(SeoController).Assembly.Location).ToString("yyyy-MM-dd");

    [HttpGet("/sitemap.xml")]
    [ResponseCache(Duration = 3600, Location = ResponseCacheLocation.Any)]
    public IActionResult Sitemap()
    {
        var image = SecurityElement.Escape(PublicUrl.Base(Request) + SiteJsonLd.OgImagePath);
        var xml = new StringBuilder();
        xml.AppendLine("""<?xml version="1.0" encoding="UTF-8"?>""");
        xml.AppendLine("""<urlset xmlns="http://www.sitemaps.org/schemas/sitemap/0.9" xmlns:xhtml="http://www.w3.org/1999/xhtml" xmlns:image="http://www.google.com/schemas/sitemap-image/1.1">""");
        foreach (var page in SitePages.All)
        {
            var en = SecurityElement.Escape(PublicUrl.Absolute(Request, page.EnglishPath));
            var es = SecurityElement.Escape(PublicUrl.Absolute(Request, page.SpanishPath));
            foreach (var loc in new[] { en, es })
            {
                xml.AppendLine("  <url>");
                xml.AppendLine($"    <loc>{loc}</loc>");
                xml.AppendLine($"""    <xhtml:link rel="alternate" hreflang="en-US" href="{en}"/>""");
                xml.AppendLine($"""    <xhtml:link rel="alternate" hreflang="es-US" href="{es}"/>""");
                xml.AppendLine($"""    <xhtml:link rel="alternate" hreflang="x-default" href="{en}"/>""");
                xml.AppendLine($"    <lastmod>{LastModified}</lastmod>");
                xml.AppendLine("    <changefreq>monthly</changefreq>");
                xml.AppendLine($"    <priority>{page.Priority}</priority>");
                if (page == SitePages.Home)
                {
                    xml.AppendLine($"    <image:image><image:loc>{image}</image:loc></image:image>");
                }
                xml.AppendLine("  </url>");
            }
        }

        xml.AppendLine("</urlset>");
        return Content(xml.ToString(), "application/xml", Encoding.UTF8);
    }

    [HttpGet("/robots.txt")]
    [ResponseCache(Duration = 3600, Location = ResponseCacheLocation.Any)]
    public IActionResult Robots()
    {
        var body =
            $"""
             User-agent: *
             Allow: /
             Disallow: /ops
             Disallow: /api/

             Sitemap: {PublicUrl.Base(Request)}/sitemap.xml
             """;
        return Content(body, "text/plain", Encoding.UTF8);
    }

    /// <summary>Plain-language summary for AI assistants and answer engines (llmstxt.org).</summary>
    [HttpGet("/llms.txt")]
    [ResponseCache(Duration = 3600, Location = ResponseCacheLocation.Any)]
    public IActionResult LlmsTxt()
    {
        var text = new StringBuilder();
        text.AppendLine($"# {SiteInfo.LegalName}");
        text.AppendLine();
        text.AppendLine("> Insured Charlotte, North Carolina remodeling and property-services company. Core service: unit turn / make-ready, coordinated by one accountable team from vacant to rent-ready, for single properties through projects of 300+ units.");
        text.AppendLine();
        text.AppendLine("- Services: unit turn / make-ready, painting, drywall, flooring, cleaning and trash-out, punch-list repairs, property maintenance, kitchen and bathroom remodeling, home renovation, exterior and common areas.");
        text.AppendLine("- Customers: property managers, multifamily teams, rental owners, remote real estate investors.");
        text.AppendLine("- Primary market: Charlotte, North Carolina.");
        text.AppendLine("- Languages: English and Spanish.");
        text.AppendLine("- Insurance: insured operations; COI documentation available during vendor onboarding.");
        text.AppendLine($"- Address: {SiteInfo.StreetAddress}, {SiteInfo.City}, {SiteInfo.Region} {SiteInfo.PostalCode}");
        text.AppendLine("- Hours: Monday–Friday, 8:00 AM–6:00 PM");
        text.AppendLine($"- Phone: {SiteInfo.PhoneDisplay}");
        text.AppendLine($"- Email: {SiteInfo.Email}");
        text.AppendLine();
        text.AppendLine("## Pages");
        text.AppendLine();
        foreach (var page in SitePages.All)
        {
            text.AppendLine($"- [{page.Title.En}]({PublicUrl.Absolute(Request, page.EnglishPath)}): {page.Description.En}");
        }

        text.AppendLine();
        text.AppendLine("## Español");
        text.AppendLine();
        foreach (var page in SitePages.All)
        {
            text.AppendLine($"- [{page.Title.Es}]({PublicUrl.Absolute(Request, page.SpanishPath)}): {page.Description.Es}");
        }

        return Content(text.ToString(), "text/plain", Encoding.UTF8);
    }
}
