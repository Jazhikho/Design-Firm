using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;

namespace Assets.Scripts.Data
{
    /// <summary>
    /// In-game credit copy for the main menu credits overlay. Team entries are grouped by function (e.g. art, programming).
    /// Shipped, player-visible third-party lines must match confirmed rows in <c>Docs/Assets.md</c> in the repository;
    /// the <see cref="ThirdPartyAttributions"/> list is the ship source (game does not read markdown at runtime).
    /// </summary>
    public static class CreditsCatalog
    {
        /// <summary>Separator used in source lines between display name and role (space, em dash, space).</summary>
        public const string NameRoleSeparator = " \u2014 ";

        /// <summary>Display heading for the AI tool disclosure block.</summary>
        public const string AiAssistanceSectionHeading = "AI ASSISTANCE";

        /// <summary>Display heading for the optional third-party block (only if <see cref="ThirdPartyAttributions"/> is non-empty).</summary>
        public const string ThirdPartySectionHeading = "THIRD-PARTY ASSETS";

        /// <summary>
        /// Team credits by functional area, in display order. Each line remains &quot;Name [separator] role(s)&quot; for the credits UI.
        /// </summary>
        public static readonly (string Heading, IReadOnlyList<string> Lines)[] TeamByFunctionSections = new (string, IReadOnlyList<string>)[]
        {
            (
                "PRODUCTION",
                new ReadOnlyCollection<string>(new string[]
                {
                    "Kevin McGhee \u2014 Lead Producer, Research Assistant, Social Media Team",
                    "Nouf Alhamoudi \u2014 Production Assistant",
                    "River Vondi \u2014 Co-Design Team Lead, Project Manager"
                })
            ),
            (
                "DESIGN",
                new ReadOnlyCollection<string>(new string[]
                {
                    "Prince Ukoha \u2014 Design Team Lead",
                    "Alex Ford \u2014 Design Team",
                    "Matthew Nichols \u2014 Design Team, QA, Repo Management"
                })
            ),
            (
                "PROGRAMMING",
                new ReadOnlyCollection<string>(new string[]
                {
                    "Jeff Fattic \u2014 Programming Lead",
                    "Rush Doebelin \u2014 Programmer",
                    "Markus Ross \u2014 Programmer"
                })
            ),
            (
                "ART",
                new ReadOnlyCollection<string>(new string[]
                {
                    "Bella Howe \u2014 Art Team Lead, Writer, Narrative",
                    "Rachel Bratton \u2014 Artist, Writer",
                    "Bianca Darlington \u2014 Artist, Writer",
                    "Jade Dorman \u2014 Artist, Audio",
                    "Logan Hentschel \u2014 Artist",
                    "Jamontae Martin \u2014 Artist",
                    "Corey Miller \u2014 Artist",
                    "Jack Murray \u2014 Artist, Writer",
                    "Sam Nienaber \u2014 Artist",
                    "Bri Ochab \u2014 Artist",
                    "Tyler Rolwes \u2014 Artist",
                    "Evan White \u2014 Artist",
                    "Kaya Wilson \u2014 Artist"
                })
            ),
            (
                "AUDIO",
                new ReadOnlyCollection<string>(new string[]
                {
                    "Ashley Haberberger \u2014 Team lead for Audio, Research, Writing, and Narrative; Production Assistant"
                })
            ),
            (
                "WRITING & NARRATIVE",
                new ReadOnlyCollection<string>(new string[]
                {
                    "Nicholas Best \u2014 Writer",
                    "Matthew Glass \u2014 Writer, Narrative, Social Media Team"
                })
            ),
            (
                "OPERATIONS, QA & RELEASE",
                new ReadOnlyCollection<string>(new string[]
                {
                    "Chris Del Gesso \u2014 Repo Manager, Product Release Lead, QA, Production Assistant, Assistant Project Manager"
                })
            )
        };

        /// <summary>Disclosure lines for development tools, each &quot;Tool [separator] short role&quot;.</summary>
        public static readonly IReadOnlyList<string> AiAssistanceLines = new ReadOnlyCollection<string>(new string[]
        {
            "Cursor \u2014 repository management and merge-patching assistance",
            "Composer 2 \u2014 credits screen prompt, support, and implementation assistance"
        });

        /// <summary>
        /// Shipped, player-visible third-party credit lines. Leave empty to hide the in-game section.
        /// Add entries that match the table in <c>Docs/Assets.md</c> (confirmed asset, source, license in repo docs; not a runtime <c>Assets.md</c> file).
        /// </summary>
        public static readonly IReadOnlyList<string> ThirdPartyAttributions = new ReadOnlyCollection<string>(Array.Empty<string>());
    }
}
