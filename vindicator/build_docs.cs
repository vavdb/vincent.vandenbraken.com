// Build the static codex + curated design-doc pages for
// vincent.vandenbraken.com/vindicator, and vendor the game's shared CSS.
//
// .NET 10 file-based script (owner ruling 2026-08-08: one language across
// game, tooling and site — this replaced the equivalent build_docs.py):
//
//   dotnet run --file vindicator/build_docs.cs
//
// Sources (read-only, from the game repo):
//   P:\Vindicator\assets\codex\*.md
//   P:\Vindicator\docs\design-*.md  (allowlist only)
//   P:\Vindicator\scripts\emit-web-css.cs  (tokens.css + components.css)
//
// Outputs (committed static files for Netlify):
//   vindicator/codex/*.html
//   vindicator/design/*.html
//   vindicator/tokens.css      — GENERATED, byte-identical to /tokens.css
//   vindicator/components.css  — GENERATED, byte-identical to /components.css
//
// The two CSS files are written by the game's own emitter, not copied or
// re-typed here: whatever the in-game state server serves is what this site
// ships (Plan/style-parity-three-surfaces.md).

#:package Markdig@0.45.0

using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using Markdig;

string Site = FindSite();
const string Game = @"P:\Vindicator";
string codexSrc = Path.Combine(Game, "assets", "codex");
string designSrc = Path.Combine(Game, "docs");

// Player-facing codex: ship all articles.
// Design docs: systems/mechanics WHY only — no story-bible / cradle spoilers.
string[] designAllowlist =
[
    "design-visual-combat-spine.md",
    "design-weapons-modules.md",
    "design-faction-art-language.md",
    "design-economy.md",
    "design-fleet.md",
    "design-fleets-content.md",
    "design-stations.md",
    "design-battle-stations.md",
    "design-conflict.md",
    "design-munitions.md",
    "design-combat-heat.md",
    "design-generation.md",
    "design-universe.md",
];

// These source documents are deliberately excluded from the published site;
// keep their references as plain text instead of emitting broken links.
string[] unavailableDocs =
[
    "art-style-guide.md",
    "story-outline.md",
    "design-bible.md",
    "assets.md",
    "survey-foozle-hull-animation.md",
];

string[] categoryOrder = ["Flight", "Universe", "Economy", "Factions", "Combat", "Other"];

MarkdownPipeline pipeline = new MarkdownPipelineBuilder()
    .UsePipeTables()
    .UseAutoIdentifiers(Markdig.Extensions.AutoIdentifiers.AutoIdentifierOptions.GitHub)
    .UseSmartyPants()
    .Build();

string builtOn = DateTime.Today.ToString("yyyy-MM-dd");

if (!Directory.Exists(codexSrc))
{
    Console.Error.WriteLine($"Game codex not found: {codexSrc}");
    return 1;
}

VendorSharedCss();

List<Article> codex = LoadCodex();
List<Article> design = LoadDesign();

string codexDir = Path.Combine(Site, "codex");
string designDir = Path.Combine(Site, "design");
// Wipe old generated pages (keep the dirs).
foreach (string dir in new[] { codexDir, designDir })
{
    if (Directory.Exists(dir))
    {
        foreach (string stale in Directory.GetFiles(dir, "*.html"))
        {
            File.Delete(stale);
        }
    }
}

WritePage(Path.Combine(codexDir, "index.html"), RenderCodexIndex(codex));
foreach (Article article in codex)
{
    WritePage(Path.Combine(codexDir, $"{article.Slug}.html"), RenderArticle(article, codex));
}

WritePage(Path.Combine(designDir, "index.html"), RenderDesignIndex(design));
foreach (Article article in design)
{
    WritePage(Path.Combine(designDir, $"{article.Slug}.html"), RenderArticle(article, design));
}

Console.WriteLine($"done: {codex.Count} codex, {design.Count} design");
return 0;

/// <summary>tokens.css + components.css come from the game's emitter so the
/// site cannot fork the palette by editing a copy. A failure here fails the
/// build: stale vendored CSS is exactly the drift this step removes.</summary>
void VendorSharedCss()
{
    string emitter = Path.Combine(Game, "scripts", "emit-web-css.cs");
    if (!File.Exists(emitter))
    {
        throw new FileNotFoundException($"game CSS emitter missing: {emitter}");
    }

    ProcessStartInfo start = new("dotnet") { WorkingDirectory = Game, RedirectStandardError = true };
    start.ArgumentList.Add("run");
    start.ArgumentList.Add("--file");
    start.ArgumentList.Add(emitter);
    start.ArgumentList.Add("--");
    start.ArgumentList.Add(Site);

    using Process process = Process.Start(start)!;
    string errors = process.StandardError.ReadToEnd();
    process.WaitForExit();
    if (process.ExitCode != 0)
    {
        throw new InvalidOperationException($"emit-web-css failed ({process.ExitCode}): {errors}");
    }

    Console.WriteLine("write tokens.css, components.css (from UiThemeTokens + VindicatorWebStyle)");
}

List<Article> LoadCodex()
{
    List<Article> articles = [];
    foreach (string path in Directory.GetFiles(codexSrc, "*.md").OrderBy(p => p, StringComparer.Ordinal))
    {
        string text = File.ReadAllText(path, Encoding.UTF8);
        (Dictionary<string, string> meta, string body) = ParseFrontmatter(text);
        string stem = Path.GetFileNameWithoutExtension(path);
        string title = FirstHeading(body, stem);
        body = StripFirstH1(body);
        string category = meta.GetValueOrDefault("category", "Other");
        if (!categoryOrder.Contains(category))
        {
            category = "Other";
        }

        int.TryParse(meta.GetValueOrDefault("order", "0"), out int order);
        articles.Add(new Article(
            stem,
            title,
            meta.GetValueOrDefault("summary", ""),
            category,
            order,
            RewriteInternalLinks(CollapseSlugDashes(CurlApostrophes(Markdown.ToHtml(body, pipeline))), "codex"),
            Path.GetFileName(path),
            "codex"));
    }

    return [.. articles.OrderBy(a => Array.IndexOf(categoryOrder, a.Category))
        .ThenBy(a => a.Order)
        .ThenBy(a => a.Title, StringComparer.Ordinal)];
}

List<Article> LoadDesign()
{
    List<Article> articles = [];
    foreach (string name in designAllowlist)
    {
        string path = Path.Combine(designSrc, name);
        if (!File.Exists(path))
        {
            Console.Error.WriteLine($"warn: missing design doc {name}");
            continue;
        }

        string text = File.ReadAllText(path, Encoding.UTF8);
        (Dictionary<string, string> meta, string body) = ParseFrontmatter(text);
        string title = FirstHeading(body, Path.GetFileNameWithoutExtension(path));
        body = StripFirstH1(body);
        string summary = meta.GetValueOrDefault("summary", "");
        if (summary.Length == 0)
        {
            // First non-empty paragraph that is neither a heading nor a bullet.
            foreach (string paragraph in Regex.Split(body, @"\n\s*\n"))
            {
                string clean = paragraph.Trim();
                if (clean.Length == 0 || clean.StartsWith('#') || clean.StartsWith('*'))
                {
                    continue;
                }

                string flat = Regex.Replace(clean, @"\s+", " ");
                summary = flat.Length > 180 ? flat[..180].TrimEnd() + "\u2026" : flat;
                break;
            }
        }

        articles.Add(new Article(
            Path.GetFileNameWithoutExtension(path),
            title,
            summary,
            "Design",
            Array.IndexOf(designAllowlist, name),
            RewriteInternalLinks(CollapseSlugDashes(CurlApostrophes(Markdown.ToHtml(body, pipeline))), "design"),
            Path.GetFileName(path),
            "design"));
    }

    return articles;
}

static (Dictionary<string, string> Meta, string Body) ParseFrontmatter(string text)
{
    Dictionary<string, string> meta = [];
    if (!text.StartsWith("---", StringComparison.Ordinal))
    {
        return (meta, text);
    }

    string[] lines = text.ReplaceLineEndings("\n").Split('\n');
    if (lines.Length == 0 || lines[0].Trim() != "---")
    {
        return (meta, text);
    }

    for (int i = 1; i < lines.Length; i++)
    {
        if (lines[i].Trim() == "---")
        {
            return (meta, string.Join("\n", lines[(i + 1)..]));
        }

        Match match = Regex.Match(lines[i], @"^\s*([^:#]+):\s*(.*?)\s*$");
        if (match.Success)
        {
            meta[match.Groups[1].Value.Trim().ToLowerInvariant()] =
                match.Groups[2].Value.Trim().Trim('"').Trim('\'');
        }
    }

    return (meta, text);
}

static string FirstHeading(string body, string fallback)
{
    foreach (string line in body.Split('\n'))
    {
        if (line.StartsWith("# ", StringComparison.Ordinal))
        {
            return line[2..].Trim();
        }
    }

    return fallback;
}

static string StripFirstH1(string body)
{
    List<string> kept = [];
    bool skipped = false;
    foreach (string line in body.Split('\n'))
    {
        if (!skipped && line.StartsWith("# ", StringComparison.Ordinal))
        {
            skipped = true;
            continue;
        }

        kept.Add(line);
    }

    return string.Join("\n", kept).TrimStart('\n');
}

/// <summary>Markdig's SmartyPants curls paired double quotes but leaves an
/// apostrophe straight, where Python-Markdown's smarty curled both. Close the
/// gap on prose only: tags, code spans and fenced blocks match first in the
/// alternation, so they come back untouched.</summary>
static string CurlApostrophes(string html) => Regex.Replace(
    html,
    @"<pre[\s\S]*?</pre>|<code[\s\S]*?</code>|<[^>]*>|(?<=\w)'",
    match => match.Value == "'" ? "&rsquo;" : match.Value);

/// <summary>Markdig's GitHub auto-identifiers emit a dash per stripped
/// character, so "Testbed + how to run it" slugs as "testbed--how-to-run-it"
/// where Python-Markdown collapsed the run. Heading ids are published
/// permalinks: keep the slugs the site already links to, fragments included.
/// </summary>
static string CollapseSlugDashes(string html)
{
    static string Slim(string slug) => Regex.Replace(slug, "-{2,}", "-").Trim('-');

    html = Regex.Replace(html, "id=\"([^\"]+)\"", m => $"id=\"{Slim(m.Groups[1].Value)}\"");
    return Regex.Replace(
        html,
        "href=\"([^\"#]*)#([^\"]+)\"",
        m => $"href=\"{m.Groups[1].Value}#{Slim(m.Groups[2].Value)}\"");
}

/// <summary>Point *.md links at the generated *.html siblings; strike through
/// repo paths that have no page on the site.</summary>
string RewriteInternalLinks(string bodyHtml, string kind)
{
    bodyHtml = Regex.Replace(bodyHtml, "<a href=\"([^\"]+)\">([\\s\\S]*?)</a>", match =>
    {
        string href = match.Groups[1].Value;
        string path = href.Split('#')[0];
        string name = path[(path.LastIndexOfAny(['/', '\\']) + 1)..];
        return unavailableDocs.Contains(name, StringComparer.OrdinalIgnoreCase)
            ? match.Groups[2].Value
            : match.Value;
    });

    return Regex.Replace(bodyHtml, "href=\"([^\"]+)\"", match =>
    {
        string full = match.Value;
        string href = match.Groups[1].Value;
        if (href.StartsWith("http://", StringComparison.Ordinal)
            || href.StartsWith("https://", StringComparison.Ordinal)
            || href.StartsWith("mailto:", StringComparison.Ordinal)
            || href.StartsWith('#'))
        {
            return full;
        }

        int hash = href.IndexOf('#');
        string path = hash < 0 ? href : href[..hash];
        string fragment = hash < 0 ? "" : href[hash..];
        string name = path[(path.LastIndexOfAny(['/', '\\']) + 1)..];

        if (name.EndsWith(".md", StringComparison.Ordinal))
        {
            string target = string.Concat(name.AsSpan(0, name.Length - 3), ".html");
            if (kind == "codex" && name.StartsWith("design-", StringComparison.Ordinal))
            {
                return full.Replace(href, $"../design/{target}{fragment}");
            }

            if (kind == "design" && !name.StartsWith("design-", StringComparison.Ordinal))
            {
                return full.Replace(href, $"../codex/{target}{fragment}");
            }

            return full.Replace(href, $"{target}{fragment}");
        }

        if (name.EndsWith(".cs", StringComparison.Ordinal)
            || name.EndsWith(".ps1", StringComparison.Ordinal)
            || path.Contains("docs/", StringComparison.Ordinal)
            || path.Contains("assets/", StringComparison.Ordinal))
        {
            return full.Replace(
                $"href=\"{href}\"",
                $"href=\"#\" title=\"Repo path (not on site): {Esc(href)}\" class=\"dead-link\"");
        }

        return full;
    });
}

string Shell(string title, string active, int depth, string content, string description)
{
    string root = string.Concat(Enumerable.Repeat("../", depth));
    string desc = description.Length > 0 ? description : title;
    string Active(string section) => active == section ? " class=\"is-active\"" : "";

    string nav = $"""
      <header class="site-nav">
        <a class="site-nav__brand" href="{root}index.html">VINDICATOR</a>
        <nav class="site-nav__links" aria-label="Section">
          <a href="{root}index.html"{Active("home")}>Home</a>
          <a href="{root}codex/index.html"{Active("codex")}>Codex</a>
          <a href="{root}design/index.html"{Active("design")}>Design</a>
          <a href="{root}design-language.html">UI language</a>
          <a href="{root}index.html#devlog">Devlog</a>
        </nav>
        <a class="site-nav__ext" href="{root}../index.html">Vincent van den Braken</a>
      </header>
    """;

    // No webfont host: vindicator.css imports fonts.css, which self-hosts the
    // settled stack from /vindicator/fonts/.
    return $"""
    <!DOCTYPE html>
    <html lang="en">
    <head>
      <meta charset="UTF-8" />
      <meta name="viewport" content="width=device-width, initial-scale=1.0" />
      <title>{Esc(title)} — Vindicator</title>
      <meta name="description" content="{Esc(desc)}" />
      <link rel="stylesheet" href="{root}vindicator.css" />
    </head>
    <body class="doc-body">
    {nav}
      <main class="doc-main">
    {content}
      </main>
      <footer class="footer">
        <span>Built {builtOn} from game sources</span>
        <span><a href="{root}index.html">Home</a> · <a href="mailto:vincent@vandenbraken.com">Contact</a></span>
        <span>© Vincent van den Braken</span>
      </footer>
    </body>
    </html>

    """;
}

string RenderCodexIndex(List<Article> articles)
{
    StringBuilder sections = new();
    foreach (string category in categoryOrder)
    {
        List<Article> items = [.. articles.Where(a => a.Category == category)];
        if (items.Count == 0)
        {
            continue;
        }

        sections.Append($"""
            <section class="doc-section">
              <h2 class="doc-section__title vtitle">{Esc(category)}</h2>
              <ul class="doc-list">
        {ListItems(items)}
              </ul>
            </section>

        """);
    }

    string body = $"""
        <header class="doc-hero">
          <span class="vchip">Codex</span>
          <h1 class="vtitle-display">In-game manual</h1>
          <p class="doc-hero__lede">Player-voiced articles that ship with the game. What the systems <em>are</em> — not the design rationale behind them.</p>
        </header>
    {sections.ToString().TrimEnd('\n')}
    """;

    return Shell("Codex", "codex", 1, body, "Vindicator codex — the living in-game manual.");
}

string RenderDesignIndex(List<Article> articles)
{
    string body = $"""
        <header class="doc-hero">
          <span class="vchip">Design</span>
          <h1 class="vtitle-display">Curated design notes</h1>
          <p class="doc-hero__lede">Systems and production WHY for a subset of design docs. Story-bible and spoiler-heavy captures stay offline. The codex still owns player-facing truth.</p>
          <p class="banner banner--warn">Developer-facing notes. May describe unshipped systems or change without notice.</p>
        </header>
        <section class="doc-section">
          <ul class="doc-list">
    {ListItems(articles)}
          </ul>
        </section>
    """;

    return Shell("Design notes", "design", 1, body, "Curated Vindicator design documents.");
}

// A list card is the shared panel pair worn by a link: the anchor is the
// chamfered rail frame (so hover lifts it via components.css), the div is
// the flat panel face. Nothing here restates a fill or a corner.
static string ListItems(List<Article> items) => string.Join("\n", items.Select(a =>
    $"""        <li><a class="vframe" href="{a.Slug}.html"><div class="vpanel">"""
    + $"""<span class="doc-list__title">{Esc(a.Title)}</span>"""
    + $"""<span class="doc-list__summary">{Esc(a.Summary)}</span></div></a></li>"""));

string RenderArticle(Article article, List<Article> siblings)
{
    bool isCodex = article.Kind == "codex";
    string warn = isCodex
        ? ""
        : """<p class="banner banner--warn">Design note — systems rationale, not player-facing codex text.</p>""";

    int index = siblings.FindIndex(s => s.Slug == article.Slug);
    string previous = index > 0
        ? $"""<a class="doc-pager__link" href="{siblings[index - 1].Slug}.html">← {Esc(siblings[index - 1].Title)}</a>"""
        : "";
    string next = index >= 0 && index < siblings.Count - 1
        ? $"""<a class="doc-pager__link" href="{siblings[index + 1].Slug}.html">{Esc(siblings[index + 1].Title)} →</a>"""
        : "";
    string summary = article.Summary.Length > 0
        ? $"""<p class="doc-article__summary">{Esc(article.Summary)}</p>"""
        : "";

    string body = $"""
        <article class="doc-article">
          <header class="doc-article__head">
            <a class="doc-back" href="index.html">← {(isCodex ? "Codex" : "Design")}</a>
            <span class="vchip">{Esc(isCodex ? article.Category : "Design")}</span>
            <h1 class="vtitle-display">{Esc(article.Title)}</h1>
            {summary}
            {warn}
          </header>
          <div class="prose">
    {article.BodyHtml.TrimEnd('\n')}
          </div>
          <nav class="doc-pager" aria-label="Adjacent articles">
            {previous}
            {next}
          </nav>
        </article>
    """;

    return Shell(
        article.Title,
        isCodex ? "codex" : "design",
        1,
        body,
        article.Summary.Length > 0 ? article.Summary : article.Title);
}

void WritePage(string path, string text)
{
    Directory.CreateDirectory(Path.GetDirectoryName(path)!);
    File.WriteAllText(path, text.ReplaceLineEndings("\n"), new UTF8Encoding(false));
    Console.WriteLine($"write {Path.GetRelativePath(Site, path).Replace('\\', '/')}");
}

/// <summary>Python's html.escape(quote=True), character for character — the
/// framework encoder also escapes every non-ASCII rune, which would turn the
/// codex's em dashes into entities.</summary>
static string Esc(string value) => value
    .Replace("&", "&amp;")
    .Replace("<", "&lt;")
    .Replace(">", "&gt;")
    .Replace("\"", "&quot;")
    .Replace("'", "&#x27;");

/// <summary>This script's own folder — the site root it writes into. Falls
/// back to walking up from the working directory if the source ever moves.</summary>
static string FindSite([CallerFilePath] string self = "")
{
    string here = Path.GetDirectoryName(self) ?? "";
    if (File.Exists(Path.Combine(here, "vindicator.css")))
    {
        return here;
    }

    DirectoryInfo? dir = new(Directory.GetCurrentDirectory());
    while (dir is not null && !File.Exists(Path.Combine(dir.FullName, "vindicator", "vindicator.css")))
    {
        dir = dir.Parent;
    }

    return dir is not null
        ? Path.Combine(dir.FullName, "vindicator")
        : throw new InvalidOperationException("run from inside the personal site repo");
}

record Article(
    string Slug,
    string Title,
    string Summary,
    string Category,
    int Order,
    string BodyHtml,
    string SourceName,
    string Kind);
