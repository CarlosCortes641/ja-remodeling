using System.Globalization;
using System.Text.Encodings.Web;
using System.Text.Unicode;
using JARemodeling.Web.Demo;
using JARemodeling.Web.Infrastructure;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.WebEncoders;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpContextAccessor();
builder.Services.AddControllersWithViews();
builder.Services.AddSingleton<LeadStore>();
builder.Services.AddSingleton<SiteJsonLd>();
builder.Services.AddScoped<SiteLanguage>();
builder.Services.Configure<WebEncoderOptions>(options =>
    options.TextEncoderSettings = new TextEncoderSettings(UnicodeRanges.All));
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});

var app = builder.Build();

app.UseForwardedHeaders();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseStaticFiles();
app.Use(async (context, next) =>
{
    var path = context.Request.Path.Value ?? "/";
    var spanish = path.Equals("/es", StringComparison.OrdinalIgnoreCase)
                  || path.StartsWith("/es/", StringComparison.OrdinalIgnoreCase);
    var culture = CultureInfo.GetCultureInfo(spanish ? "es-US" : "en-US");
    CultureInfo.CurrentCulture = culture;
    CultureInfo.CurrentUICulture = culture;
    await next();
});
app.UseRouting();
app.UseAuthorization();

app.MapControllers();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
