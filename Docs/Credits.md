# Credits

## Project

**Fashion Styling Game**  
Interactive styling game developed to support Dr. Trawick's History of Fashion course.

## Studio and Ownership

- **Studio:** Sprint 53 Studio
- **Primary studio page:** https://www.instagram.com/sprint53studio
- **Repository license:** MIT (see `LICENSE`)

## Academic Context

- **Client / course context:** Dr. Trawick's History of Fashion course
- **Core eras covered:** 1980s, 1990s, 2000s, 2010s
- **Project brief and requirements:** `Docs/ClientSpec.md`

## Research and Reference Sources

Fashion-history references used for course-aligned era coverage are tracked in:

- `Docs/Sources.md`

## Third-Party Assets and Attribution

Third-party asset attribution and license tracking are maintained in:

- `Docs/Assets.md`

## AI Assistance Disclosure

AI-assisted work disclosures and human-audit notes are maintained in:

- `Docs/AI-Provenance-Log.md`
- `AI-Use-Statement.md`

When listing assistant tools in in-game, marketing, or ship credits, align with `CreditsCatalog.AiAssistanceNames` (currently **Cursor**, **Composer 2**, **Claude**) and expand disclosures in `Docs/AI-Provenance-Log.md` as needed.

## Team and Role Credits

The **in-game** main menu “Credits” screen reads team data from `Assets/Scripts/Data/CreditsCatalog.cs`, grouped by **function** via `TeamByFunctionSections`: each line is a **person’s name** (no role subtitle). The same person appears **again** in another section when their contributions span that team. The AI block uses `AiAssistanceNames` (tool names). A “Third-Party Assets” block appears **only** when `ThirdPartyCredits` is non-empty; each entry is **asset name**, **source** (plain text, no URLs in-game), **license**, and **usage**. Full URLs stay in `Docs/Assets.md` (the game does not read markdown at runtime).

Current team names and role assignments in external team docs are also linked from `README.md` under **External Docs and Tools**.
