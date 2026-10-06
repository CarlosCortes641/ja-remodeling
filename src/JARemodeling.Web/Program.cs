using System.Globalization;
using System.IO.Compression;
using System.Text.Encodings.Web;
using System.Text.Unicode;
using JARemodeling.Web.Demo;
using JARemodeling.Web.Infrastructure;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.Extensions.WebEncoders;
using Microsoft.Net.Http.Headers;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpContextAccessor();
builder.Services.AddControllersWithViews();
builder.Services.AddSingleton<LeadStore>();
builder.Services.AddSingleton<FaqCatalog>();
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
builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
    options.Providers.Add<BrotliCompressionProvider>();
    options.Providers.Add<GzipCompressionProvider>();
    options.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(["image/svg+xml", "application/ld+json"]);
});
builder.Services.Configure<BrotliCompressionProviderOptions>(o => o.Level = CompressionLevel.Fastest);
builder.Services.Configure<GzipCompressionProviderOptions>(o => o.Level = CompressionLevel.Fastest);

var app = builder.Build();

PublicUrl.Configure(app.Configuration["Seo:SiteUrl"]);

app.UseForwardedHeaders();
app.UseResponseCompression();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/error");
    app.UseHsts();
}

app.UseStatusCodePagesWithReExecute("/not-found");

// One URL per page: /es/ and /property-managers/ redirect to their canonical form.
app.Use(async (context, next) =>
{
    var path = context.Request.Path.Value;
    if (path is { Length: > 1 } && path.EndsWith('/') && HttpMethods.IsGet(context.Request.Method))
    {
        context.Response.Redirect(path.TrimEnd('/') + context.Request.QueryString, permanent: true);
        return;
    }

    await next();
});

app.UseStaticFiles(new StaticFileOptions
{
    OnPrepareResponse = ctx =>
    {
        // asp-append-version adds ?v=hash, so versioned assets can be cached for a year.
        var versioned = ctx.Context.Request.Query.ContainsKey("v");
        ctx.Context.Response.Headers[HeaderNames.CacheControl] = versioned
            ? "public, max-age=31536000, immutable"
            : "public, max-age=86400";
    }
});

app.Use(async (context, next) =>
{
    var culture = CultureInfo.GetCultureInfo(SiteLanguage.IsSpanishPath(context.Request.Path.Value) ? "es-US" : "en-US");
    CultureInfo.CurrentCulture = culture;
    CultureInfo.CurrentUICulture = culture;
    await next();
});
app.UseRouting();
app.UseAuthorization();

app.MapControllers();

app.Run();
