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

## Publicar (gratis en Render)

Blueprint en [Render](https://dashboard.render.com/blueprint/new?repo=https://github.com/CarlosCortes641/ja-remodeling) con `render.yaml` + `Dockerfile`. El plan free se duerme sin tráfico.

## Prototipo de referencia

https://ja-remodeling-turns.jrricardo29.chatgpt.site/
