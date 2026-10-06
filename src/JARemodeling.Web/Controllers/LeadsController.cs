using System.Text.Json;
using JARemodeling.Web.Demo;
using Microsoft.AspNetCore.Mvc;

namespace JARemodeling.Web.Controllers;

[ApiController]
public class LeadsController : ControllerBase
{
    private const int MaxFieldLength = 4000;
    private readonly LeadStore _leads;

    public LeadsController(LeadStore leads) => _leads = leads;

    [HttpPost("/api/leads")]
    public IActionResult Create([FromBody] JsonElement body)
    {
        if (body.ValueKind != JsonValueKind.Object)
        {
            return BadRequest(new { error = "Invalid request." });
        }

        var fields = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);
        foreach (var property in body.EnumerateObject())
        {
            fields[property.Name] = Truncate(Flatten(property.Value));
        }

        var spanish = string.Equals(fields.GetValueOrDefault("locale"), "es", StringComparison.OrdinalIgnoreCase);

        if (string.IsNullOrWhiteSpace(fields.GetValueOrDefault("name"))
            || string.IsNullOrWhiteSpace(fields.GetValueOrDefault("email"))
            || string.IsNullOrWhiteSpace(fields.GetValueOrDefault("propertyAddress")))
        {
            return BadRequest(new
            {
                error = spanish
                    ? "Faltan nombre, correo o dirección de la propiedad."
                    : "Name, email and property address are required."
            });
        }

        if (!string.Equals(fields.GetValueOrDefault("consent"), "true", StringComparison.OrdinalIgnoreCase))
        {
            return BadRequest(new
            {
                error = spanish ? "Se requiere su consentimiento." : "Consent is required."
            });
        }

        var lead = _leads.Add(fields);
        return Ok(new { reference = lead.Id });
    }

    private static string? Flatten(JsonElement value) => value.ValueKind switch
    {
        JsonValueKind.String => value.GetString(),
        JsonValueKind.Null or JsonValueKind.Undefined => null,
        JsonValueKind.True => "true",
        JsonValueKind.False => "false",
        JsonValueKind.Object => string.Join("; ", value.EnumerateObject()
            .Select(p => (p.Name, Value: Flatten(p.Value)))
            .Where(p => !string.IsNullOrWhiteSpace(p.Value))
            .Select(p => $"{p.Name}: {p.Value}")),
        _ => value.GetRawText()
    };

    private static string? Truncate(string? value) =>
        value is { Length: > MaxFieldLength } ? value[..MaxFieldLength] : value;
}
