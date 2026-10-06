using System.Diagnostics;
using JARemodeling.Web.Infrastructure;
using JARemodeling.Web.Models;
using Microsoft.AspNetCore.Mvc;

namespace JARemodeling.Web.Controllers;

public class HomeController : Controller
{
    [HttpGet("/")]
    [HttpGet("/es")]
    public IActionResult Index() => Page(SitePages.Home, "Index");

    [HttpGet("/property-managers")]
    [HttpGet("/es/administradores")]
    public IActionResult PropertyManagers() => Page(SitePages.PropertyManagers, "PropertyManagers");

    [HttpGet("/multifamily")]
    [HttpGet("/es/multifamily")]
    public IActionResult Multifamily() => Page(SitePages.Multifamily, "Multifamily");

    [HttpGet("/rental-owners")]
    [HttpGet("/es/propietarios")]
    public IActionResult RentalOwners() => Page(SitePages.RentalOwners, "RentalOwners");

    [HttpGet("/remote-investors")]
    [HttpGet("/es/inversionistas-remotos")]
    public IActionResult RemoteInvestors() => Page(SitePages.RemoteInvestors, "RemoteInvestors");

    [Route("/error")]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error() =>
        View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });

    [Route("/not-found")]
    public IActionResult NotFoundPage()
    {
        Response.StatusCode = StatusCodes.Status404NotFound;
        return View("NotFound");
    }

    private ViewResult Page(SitePage page, string view)
    {
        ViewData["Page"] = page;
        return View(view);
    }
}
