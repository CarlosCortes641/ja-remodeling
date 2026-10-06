using System.Collections.Concurrent;

namespace JARemodeling.Web.Demo;

public sealed class LeadRecord
{
    public string Id { get; init; } = "";
    public DateTimeOffset CreatedAt { get; init; }
    public string Source { get; init; } = "";
    public string Name { get; init; } = "";
    public string Email { get; init; } = "";
    public string Phone { get; init; } = "";
    public string Language { get; init; } = "";
    public string Summary { get; init; } = "";
    public Dictionary<string, string> Fields { get; init; } = new();
}

public sealed class LeadStore
{
    private readonly ConcurrentDictionary<string, LeadRecord> _leads = new();
    private int _sequence;

    public LeadRecord Add(IDictionary<string, string?> fields)
    {
        var clean = fields
            .Where(kv => !string.IsNullOrWhiteSpace(kv.Value))
            .ToDictionary(kv => kv.Key, kv => kv.Value!.Trim(), StringComparer.OrdinalIgnoreCase);

        var id = $"JA-{DateTime.UtcNow:yyyyMMdd}-{Interlocked.Increment(ref _sequence):000}";
        var summary = string.Join(" · ", new[]
        {
            clean.GetValueOrDefault("buyerType"),
            clean.GetValueOrDefault("propertyType"),
            clean.GetValueOrDefault("unitCount") is { } units ? $"{units} units" : null,
            clean.GetValueOrDefault("propertyAddress")
        }.Where(s => !string.IsNullOrWhiteSpace(s)));

        var lead = new LeadRecord
        {
            Id = id,
            CreatedAt = DateTimeOffset.UtcNow,
            Source = clean.GetValueOrDefault("source") ?? "",
            Name = clean.GetValueOrDefault("name") ?? "",
            Email = clean.GetValueOrDefault("email") ?? "",
            Phone = clean.GetValueOrDefault("phone") ?? "",
            Language = clean.GetValueOrDefault("locale") ?? "",
            Summary = summary,
            Fields = clean
        };

        _leads[id] = lead;
        return lead;
    }

    public IReadOnlyList<LeadRecord> All() =>
        _leads.Values.OrderByDescending(l => l.CreatedAt).ToList();
}
