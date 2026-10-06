using System.Text.Json;

namespace JARemodeling.Web.Infrastructure;

public sealed record FaqItem(LocalizedText Question, LocalizedText Answer);

/// <summary>Single source for the visible FAQ and its FAQPage structured data.</summary>
public sealed class FaqCatalog
{
    public FaqCatalog(IWebHostEnvironment env)
    {
        var json = File.ReadAllText(Path.Combine(env.ContentRootPath, "Data", "faq.json"));
        var raw = JsonSerializer.Deserialize<List<RawItem>>(json, new JsonSerializerOptions(JsonSerializerDefaults.Web))!;
        Items = raw.Select(r => new FaqItem(new(r.En.Q, r.Es.Q), new(r.En.A, r.Es.A))).ToList();
    }

    public IReadOnlyList<FaqItem> Items { get; }

    private sealed record RawText(string Q, string A);
    private sealed record RawItem(RawText En, RawText Es);
}
