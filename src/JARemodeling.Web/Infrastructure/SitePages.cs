namespace JARemodeling.Web.Infrastructure;

public sealed record LocalizedText(string En, string Es)
{
    public string For(bool spanish) => spanish ? Es : En;
}

public sealed record SitePage(
    string Key,
    string EnglishPath,
    string SpanishPath,
    string Priority,
    LocalizedText Title,
    LocalizedText Description,
    LocalizedText Name,
    LocalizedText CardText,
    string? AudienceType = null)
{
    public string PathFor(bool spanish) => spanish ? SpanishPath : EnglishPath;
    public bool IsAudience => AudienceType is not null;
}

public static class SitePages
{
    public static readonly SitePage Home = new(
        "home", "/", "/es", "1.0",
        new("Unit Turn & Make-Ready in Charlotte, NC | J&A Remodeling",
            "Unit Turns y Make-Ready en Charlotte, NC | J&A Remodeling"),
        new("Insured Charlotte remodeling company for unit turns, make-ready, painting, drywall, flooring and renovations. One accountable team from vacant to rent-ready.",
            "Empresa asegurada en Charlotte para unit turns, make-ready, pintura, drywall, pisos, limpieza y renovaciones. Un solo equipo de vacante a lista para rentar."),
        new("J&A Remodeling", "J&A Remodeling"),
        new("Unit turns, make-ready, painting, drywall, flooring, cleaning and renovation support.",
            "Unit turns, make-ready, pintura, drywall, pisos, limpieza y renovaciones."));

    public static readonly SitePage PropertyManagers = new(
        "property-managers", "/property-managers", "/es/administradores", "0.8",
        new("Make-Ready for Property Managers in Charlotte, NC | J&A",
            "Make-ready para administradores en Charlotte, NC | J&A"),
        new("Charlotte make-ready and unit turn vendor for property managers: work orders to itemized scope, approval control, phased crews and documented close-out.",
            "Proveedor de make-ready y unit turns en Charlotte para administradores: órdenes de trabajo, alcance detallado, aprobaciones y cierre documentado."),
        new("Property managers", "Administradores de propiedades"),
        new("Repeatable scopes, work-order discipline, approval control and cleaner invoicing.",
            "Alcances repetibles, órdenes de trabajo, aprobación y facturación más clara."),
        "Property managers");

    public static readonly SitePage Multifamily = new(
        "multifamily", "/multifamily", "/es/multifamily", "0.8",
        new("Multifamily Unit Turns & Make-Ready in Charlotte, NC | J&A",
            "Unit turns para multifamiliares en Charlotte, NC | J&A"),
        new("Phased multifamily unit turns in Charlotte: down units to 300+ unit programs, grouped by condition and leasing priority with consistent finish standards.",
            "Unit turns multifamiliares por fases en Charlotte: desde una down unit hasta programas de 300+ unidades, según condición y prioridad de leasing."),
        new("Multifamily teams", "Equipos multifamiliares"),
        new("Phased crews, leasing-priority sequencing and consistent standards across communities.",
            "Equipos por fases, prioridad de leasing y estándares consistentes."),
        "Multifamily operators");

    public static readonly SitePage RentalOwners = new(
        "rental-owners", "/rental-owners", "/es/propietarios", "0.8",
        new("Rental Property Turnover & Make-Ready in Charlotte, NC | J&A",
            "Preparación de casas de alquiler en Charlotte, NC | J&A"),
        new("Get your Charlotte rental from move-out to show-ready: inspection, required vs. optional repairs, paint, flooring, cleaning and a documented handoff.",
            "Prepare su propiedad de alquiler en Charlotte después de la mudanza: inspección, reparaciones necesarias y opcionales, pintura, pisos, limpieza y entrega."),
        new("Rental owners", "Propietarios locales"),
        new("A guided path from move-out condition to a home ready to show and lease.",
            "Una ruta guiada desde la salida del inquilino hasta lista para rentar."),
        "Rental property owners");

    public static readonly SitePage RemoteInvestors = new(
        "remote-investors", "/remote-investors", "/es/inversionistas-remotos", "0.8",
        new("Make-Ready for Remote Rental Investors in Charlotte | J&A",
            "Make-ready para inversionistas remotos en Charlotte | J&A"),
        new("Own a Charlotte rental from out of state? J&A coordinates access, written approvals, milestone updates and photo close-out so you never need to be onsite.",
            "¿Invierte en Charlotte desde otro estado? J&A coordina acceso, aprobaciones escritas, actualizaciones y fotos de cierre sin que tenga que estar presente."),
        new("Remote investors", "Inversionistas remotos"),
        new("Local coordination, written approvals, progress visibility and documented close-out.",
            "Coordinación local, aprobaciones, visibilidad y cierre documentado."),
        "Remote real estate investors");

    public static readonly IReadOnlyList<SitePage> All = [Home, PropertyManagers, Multifamily, RentalOwners, RemoteInvestors];
    public static readonly IReadOnlyList<SitePage> Audiences = All.Where(p => p.IsAudience).ToList();
}
