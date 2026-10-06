using System.Security;
using System.Text;
using JARemodeling.Web.Infrastructure;
using Microsoft.AspNetCore.Mvc;

namespace JARemodeling.Web.Controllers;

public class SeoController : Controller
{
    [HttpGet("/sitemap.xml")]
    [ResponseCache(Duration = 3600, Location = ResponseCacheLocation.Any)]
    public IActionResult Sitemap()
    {
        var lastmod = DateTime.UtcNow.ToString("yyyy-MM-dd");
        var xml = new StringBuilder();
        xml.AppendLine("""<?xml version="1.0" encoding="UTF-8"?>""");
        xml.AppendLine("""<urlset xmlns="http://www.sitemaps.org/schemas/sitemap/0.9" xmlns:xhtml="http://www.w3.org/1999/xhtml">""");
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
                xml.AppendLine($"    <lastmod>{lastmod}</lastmod>");
                xml.AppendLine("    <changefreq>monthly</changefreq>");
                xml.AppendLine($"    <priority>{page.Priority}</priority>");
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
}
