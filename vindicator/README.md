# vincent.vandenbraken.com/vindicator

Static landing + generated **Codex** and curated **Design** docs.

## Regenerate docs

Requires the game repo at `P:\Vindicator` and the .NET 10 SDK. No Python:
the build is a file-based C# script, one language across game, tooling and
site.

```powershell
dotnet run --file vindicator/build_docs.cs
```

Sources:

| Site | Game path |
|------|-----------|
| `/vindicator/codex/*` | `P:\Vindicator\assets\codex\*.md` (all) |
| `/vindicator/design/*` | allowlisted `P:\Vindicator\docs\design-*.md` |
| `/vindicator/tokens.css` | `UiThemeTokens.ToCssVariables()` |
| `/vindicator/components.css` | `VindicatorWebStyle.Components` |

Excluded from design by default: `design-bible.md`, `design-cradle.md` (story spoilers).

Generated HTML is committed so Netlify can deploy without the game tree.

**Never hand-edit `tokens.css` or `components.css`.** The build runs the
game's own emitter (`P:\Vindicator\scripts\emit-web-css.cs`), so both files
are byte-identical to what the in-game state server serves at `/tokens.css`
and `/components.css`. A palette change is a change to `UiThemeTokens`,
followed by a rebuild here.

## Local preview

```powershell
cd P:\vincent.vandenbraken.com
npx --yes serve -p 4173
# open http://localhost:4173/vindicator/
```
