# J&A Remodeling

Website comercial para J&A Remodeling and Services LLC (unit turns y make-ready en Charlotte, NC), basado en el prototipo ChatGPT Sites.

Stack: **ASP.NET Core 8 MVC + Razor + CSS + JavaScript** — mismo enfoque que `majestic-roofing` / `manuela-ruiz-realty` / `safe-project`. El prototipo queda como especificación visual; no es la base de producción.

Por agilidad, **no hay base de datos**. Los leads de formularios viven en memoria (`LeadStore`) y se ven en `/ops`.

## Correr

Requisito: .NET 8 SDK.

```bash
cd /Users/mac/Projects/ja-remodeling
dotnet run --project src/JARemodeling.Web --urls http://localhost:5132
```

Abre `http://localhost:5132`.

## Qué incluye

1. Home alineada al prototipo (hero, empresa, clientes, servicios, tipos de propiedad, planificación, capacidad 300+ unidades, planner de 6 pasos, recursos locales, FAQ, contacto)
2. Páginas por tipo de cliente: `/property-managers`, `/multifamily`, `/rental-owners`, `/remote-investors`
3. Idioma por URL (inglés por defecto, español bajo `/es`): `/es`, `/es/administradores`, `/es/multifamily`, `/es/propietarios`, `/es/inversionistas-remotos`
4. Formulario de solicitud y planner de 6 pasos → `POST /api/leads` → `LeadStore` in-memory → tablero `/ops`
5. SEO: canonical, hreflang EN/ES, Open Graph, JSON-LD `LocalBusiness`, `/sitemap.xml`, `/robots.txt`

**No activado:** CRM, email transaccional ni almacenamiento persistente de leads.

## Regenerar vistas desde el prototipo

Las vistas de `Views/Home` y `Views/Shared/_LeadForm.cshtml` se generan fusionando el HTML EN y ES del prototipo (`Lang.T("en", "es")`). El cache del prototipo vive en `.proto-cache/` (ignorado por git).

```bash
python3 scripts/build_views.py
```

## Publicar (Azure App Service)

Cada push a `main` despliega con GitHub Actions (`.github/workflows/deploy-azure.yml`) al App Service `jaremodeling` (plan compartido `ASP-recursoproduccion-94d0`, grupo `recursoproduccion`).

El secreto `AZURE_WEBAPP_PUBLISH_PROFILE` ya está configurado en el repo. Si hay que renovarlo:

1. En Azure Portal → App Service `jaremodeling` → **Get publish profile** → descarga el `.PublishSettings`
2. En GitHub → repo → **Settings → Secrets and variables → Actions** → actualiza `AZURE_WEBAPP_PUBLISH_PROFILE` con el contenido completo del archivo
3. Push a `main` (o **Actions → Deploy J&A Remodeling to Azure → Run workflow**)

App URL: `https://jaremodeling.azurewebsites.net`

## Publicar (gratis en Render)

Alternativa: Blueprint en [Render](https://dashboard.render.com/blueprint/new?repo=https://github.com/CarlosCortes641/ja-remodeling) con `render.yaml` + `Dockerfile`. El plan free se duerme sin tráfico.

## Prototipo de referencia

https://ja-remodeling-turns.jrricardo29.chatgpt.site/
