#!/usr/bin/env python3
"""Generate bilingual Razor views from the ChatGPT Sites prototype cache.

The prototype renders the same component tree for English and Spanish, so the
two server-rendered <main> elements align token by token. Identical tokens are
emitted verbatim; differing text/attributes become @Lang.T("en", "es").
"""
from __future__ import annotations

import html
import json
import re
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
CACHE = ROOT / ".proto-cache"
WEB = ROOT / "src" / "JARemodeling.Web"
PROTO_HOST = "https://ja-remodeling-turns.jrricardo29.chatgpt.site"

PAGES = [
    # (view name, english cache file, spanish cache file, page key, default buyer)
    ("Index", "index.html", "es.html", "home", "property-manager"),
    ("PropertyManagers", "pages/property-managers.html", "pages/es_administradores.html", "property-managers", "property-manager"),
    ("Multifamily", "pages/multifamily.html", "pages/es_multifamily.html", "multifamily", "multifamily"),
    ("RentalOwners", "pages/rental-owners.html", "pages/es_propietarios.html", "rental-owners", "rental-owner"),
    ("RemoteInvestors", "pages/remote-investors.html", "pages/es_inversionistas-remotos.html", "remote-investors", "remote-investor"),
]

BLOCK = {
    "section", "div", "header", "footer", "article", "form", "nav", "dl", "dt", "dd",
    "ul", "li", "details", "summary", "fieldset", "legend", "aside", "label", "p",
    "h1", "h2", "h3", "select", "option", "textarea", "button", "address", "small",
}
VOID = {"input", "br", "img", "hr", "meta", "link"}

TOKEN = re.compile(r"(<!--.*?-->|<[^>]+>)", re.S)
ATTR = re.compile(r'([^\s=/]+)(?:="([^"]*)")?')


def tokens(markup: str) -> list[str]:
    out: list[str] = []
    for t in TOKEN.split(markup):
        if not t or t.startswith("<!--"):
            continue
        if out and not t.startswith("<") and not out[-1].startswith("<"):
            out[-1] += t
        else:
            out.append(t)
    return out


def tag_name(tok: str) -> str:
    return re.match(r"</?\s*([a-zA-Z0-9]+)", tok).group(1).lower()


def cs(value: str) -> str:
    return '"' + html.unescape(value).replace("\\", "\\\\").replace('"', '\\"') + '"'


def razor_literal(value: str) -> str:
    return value.replace("@", "@@")


def lang_t(en: str, es: str) -> str:
    return f"@Lang.T({cs(en)}, {cs(es)})"


def parse_tag(tok: str) -> tuple[str, list[tuple[str, str | None]], bool]:
    inner = tok[1:-1].strip()
    self_closing = inner.endswith("/")
    inner = inner.rstrip("/").strip()
    name, _, rest = inner.partition(" ")
    return name, [(m.group(1), m.group(2)) for m in ATTR.finditer(rest)], self_closing


def render_tag(name: str, attrs: list[tuple[str, str]], self_closing: bool) -> str:
    parts = [name] + [a if v is None else f'{a}="{v}"' for a, v in attrs]
    return "<" + " ".join(parts) + (" />" if self_closing else ">")


def merge_tag(en: str, es: str) -> str:
    if en == es:
        return razor_literal(en)
    name, a_en, sc = parse_tag(en)
    _, a_es, _ = parse_tag(es)
    es_map = dict(a_es)
    merged = []
    for key, value in a_en:
        other = es_map.get(key, value)
        if value == other:
            merged.append((key, None if value is None else razor_literal(value)))
        else:
            merged.append((key, lang_t(value or "", other or "")))
    return render_tag(name, merged, sc)


def merge(en_tokens: list[str], es_tokens: list[str]) -> list[str]:
    assert len(en_tokens) == len(es_tokens), (len(en_tokens), len(es_tokens))
    out = []
    for en, es in zip(en_tokens, es_tokens):
        if en.startswith("<"):
            assert tag_name(en) == tag_name(es), (en, es)
            out.append(merge_tag(en, es))
        elif en == es:
            out.append(razor_literal(en))
        else:
            lead = re.match(r"\s*", en).group(0)
            trail = en[len(en.rstrip()):]
            out.append(lead + lang_t(en.strip(), es.strip()) + trail)
    return out


def format_tokens(toks: list[str], depth: int = 0) -> str:
    """Put block elements on their own indented lines; keep inline content inline."""
    out: list[str] = []
    last_was_block_close = False
    for tok in toks:
        if tok.startswith("</"):
            name = tag_name(tok)
            if name in BLOCK:
                depth -= 1
                if last_was_block_close:
                    out.append("\n" + "    " * depth)
                out.append(tok)
                last_was_block_close = True
                continue
        elif tok.startswith("<"):
            name = tag_name(tok)
            if name in BLOCK and not tok.endswith("/>"):
                out.append("\n" + "    " * depth + tok)
                depth += 1
                last_was_block_close = False
                continue
        out.append(tok)
        last_was_block_close = False
    return "".join(out).strip() + "\n"


def find_element(toks: list[str], predicate) -> tuple[int, int]:
    """Return [start, end] token indexes of the first element whose open tag matches."""
    for i, tok in enumerate(toks):
        if tok.startswith("<") and not tok.startswith("</") and predicate(tok):
            name = tag_name(tok)
            depth = 0
            for j in range(i, len(toks)):
                t = toks[j]
                if t.startswith("<") and tag_name(t) == name and not t.endswith("/>"):
                    depth += -1 if t.startswith("</") else 1
                    if depth == 0:
                        return i, j
    raise ValueError("element not found")


def main_markup(path: Path) -> tuple[str, str]:
    doc = path.read_text(errors="ignore")
    match = re.search(r"<main([^>]*)>(.*)</main>", doc, re.S)
    cls = re.search(r'class="([^"]*)"', match.group(1))
    return (cls.group(1) if cls else ""), match.group(2)


FAQ_ITEM = re.compile(
    r"<details[^>]*><summary><span>[^<]*(?:<!-- -->[^<]*)?</span>(.*?)<b>\+</b></summary><p>(.*?)</p></details>",
    re.S,
)


def faq_items(main: str) -> list[dict[str, str]]:
    block = re.search(r'<div class="faqList">(.*?)</div></section>', main, re.S).group(1)
    return [{"q": html.unescape(q), "a": html.unescape(a)} for q, a in FAQ_ITEM.findall(block)]


def build_lead_form_partial(form_toks: list[str]) -> str:
    body = format_tokens(form_toks)
    body = body.replace(
        '<form class="leadForm">',
        '<form class="leadForm" data-lead-form data-source="@Model.Source">',
    )
    body = re.sub(
        r'<option value="([a-z-]+)"(?: selected="")?>',
        lambda m: f'<option value="{m.group(1)}" selected="@(Model.DefaultBuyer == "{m.group(1)}")">',
        body,
    )
    return "@model JARemodeling.Web.ViewModels.LeadFormOptions\n" + body


def build() -> None:
    views = WEB / "Views" / "Home"
    views.mkdir(parents=True, exist_ok=True)
    (WEB / "Data").mkdir(exist_ok=True)
    partial_written = False

    for view, en_file, es_file, key, buyer in PAGES:
        main_class, en_main = main_markup(CACHE / en_file)
        _, es_main = main_markup(CACHE / es_file)
        merged = merge(tokens(en_main), tokens(es_main))

        start, end = find_element(merged, lambda t: t.startswith('<form class="leadForm"'))
        if not partial_written:
            partial = build_lead_form_partial(merged[start:end + 1])
            (WEB / "Views" / "Shared" / "_LeadForm.cshtml").write_text(partial)
            partial_written = True
        source = "homepage" if key == "home" else key
        merged[start:end + 1] = [
            f'<partial name="_LeadForm" model="@(new LeadFormOptions("{buyer}", "{source}"))" />'
        ]

        merged = [
            '<form data-turn-planner>' if t == "<form>" else t
            for t in merged
        ]

        if key == "home":
            en_faq, es_faq = faq_items(en_main), faq_items(es_main)
            assert en_faq and len(en_faq) == len(es_faq)
            faq = [{"en": e, "es": s} for e, s in zip(en_faq, es_faq)]
            (WEB / "Data" / "faq.json").write_text(json.dumps(faq, ensure_ascii=False, indent=2) + "\n")
            start, end = find_element(merged, lambda t: t == '<div class="faqList">')
            merged[start:end + 1] = ['<partial name="_FaqList" />']
        else:
            contact = merged.index('<section class="contactSection" id="request">')
            merged.insert(contact, '<partial name="_RelatedAudiences" />')

        # Title and meta description come from SitePages (unique per page and language).
        header = (
            "@{\n"
            f"    ViewData[\"MainClass\"] = {cs(main_class)};\n"
            "}\n"
        )
        (views / f"{view}.cshtml").write_text(header + format_tokens(merged))
        print(f"wrote Views/Home/{view}.cshtml ({len(merged)} tokens)")

    doc = (CACHE / "index.html").read_text(errors="ignore")
    ld = json.loads(re.search(r'<script type="application/ld\+json">(.*?)</script>', doc, re.S).group(1))
    text = json.dumps(ld, ensure_ascii=False, indent=2).replace(PROTO_HOST, "{{BASE_URL}}")
    data = WEB / "Data"
    data.mkdir(exist_ok=True)
    (data / "business.jsonld.json").write_text(text + "\n")
    print("wrote Data/business.jsonld.json")


if __name__ == "__main__":
    build()
