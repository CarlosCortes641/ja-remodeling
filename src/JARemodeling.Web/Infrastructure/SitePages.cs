namespace JARemodeling.Web.Infrastructure;

public sealed record SitePage(string Key, string EnglishPath, string SpanishPath, string Priority);

public static class SitePages
{
    public static readonly SitePage Home = new("home", "/", "/es", "1.0");
    public static readonly SitePage PropertyManagers = new("property-managers", "/property-managers", "/es/administradores", "0.8");
    public static readonly SitePage Multifamily = new("multifamily", "/multifamily", "/es/multifamily", "0.8");
    public static readonly SitePage RentalOwners = new("rental-owners", "/rental-owners", "/es/propietarios", "0.8");
    public static readonly SitePage RemoteInvestors = new("remote-investors", "/remote-investors", "/es/inversionistas-remotos", "0.8");

    public static readonly IReadOnlyList<SitePage> All = [Home, PropertyManagers, Multifamily, RentalOwners, RemoteInvestors];

    public static SitePage ByKey(string? key) => All.FirstOrDefault(p => p.Key == key) ?? Home;
}
