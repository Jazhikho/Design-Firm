using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;

namespace Assets.Scripts.Data
{
    /// <summary>
    /// In-game credits: team names grouped by function. Each <see cref="TeamByFunctionSections"/> list entry is
    /// a single person (names only, no per-person role sub-lines). The same name may appear in more than one section
    /// when the person’s roles span those teams. Third-party and AI lists follow the same in-game string rules in code.
    /// </summary>
    public static class CreditsCatalog
    {
        /// <summary>Display heading for the AI tool disclosure block.</summary>
        public const string AiAssistanceSectionHeading = "AI ASSISTANCE";

        /// <summary>Display heading for the optional third-party block (only if <see cref="ThirdPartyAttributions"/> is non-empty).</summary>
        public const string ThirdPartySectionHeading = "THIRD-PARTY ASSETS";

        /// <summary>Team by function: section heading and a list of person names (one label per name).</summary>
        public static readonly (string Heading, IReadOnlyList<string> Lines)[] TeamByFunctionSections = new (string, IReadOnlyList<string>)[]
        {
            (
                "PRODUCTION",
                new ReadOnlyCollection<string>(new string[]
                {
                    "Kevin McGhee",
                    "Nouf Alhamoudi",
                    "River Vondi"
                })
            ),
            (
                "DESIGN",
                new ReadOnlyCollection<string>(new string[]
                {
                    "Prince Ukoha",
                    "Alex Ford",
                    "Matthew Nichols",
                    "River Vondi"
                })
            ),
            (
                "PROGRAMMING",
                new ReadOnlyCollection<string>(new string[]
                {
                    "Jeff Fattic",
                    "Rush Doebelin",
                    "Markus Ross"
                })
            ),
            (
                "ART",
                new ReadOnlyCollection<string>(new string[]
                {
                    "Bella Howe",
                    "Rachel Bratton",
                    "Bianca Darlington",
                    "Jade Dorman",
                    "Logan Hentschel",
                    "Jamontae Martin",
                    "Corey Miller",
                    "Jack Murray",
                    "Sam Nienaber",
                    "Bri Ochab",
                    "Tyler Rolwes",
                    "Evan White",
                    "Kaya Wilson"
                })
            ),
            (
                "AUDIO",
                new ReadOnlyCollection<string>(new string[]
                {
                    "Ashley Haberberger",
                    "Jade Dorman"
                })
            ),
            (
                "WRITING & NARRATIVE",
                new ReadOnlyCollection<string>(new string[]
                {
                    "Nicholas Best",
                    "Matthew Glass",
                    "Bella Howe",
                    "Rachel Bratton",
                    "Bianca Darlington",
                    "Jack Murray"
                })
            ),
            (
                "OPERATIONS, QA & RELEASE",
                new ReadOnlyCollection<string>(new string[]
                {
                    "Chris Del Gesso"
                })
            )
        };

        /// <summary>AI tools (name-only lines, same as team rows).</summary>
        public static readonly IReadOnlyList<string> AiAssistanceNames = new ReadOnlyCollection<string>(new string[]
        {
            "Cursor",
            "Composer 2"
        });

        /// <summary>
        /// Shipped, player-visible third-party credit lines. Leave empty to hide the in-game section.
        /// </summary>
        public static readonly IReadOnlyList<string> ThirdPartyAttributions = new ReadOnlyCollection<string>(Array.Empty<string>());
    }
}
