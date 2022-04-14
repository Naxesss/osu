// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using osu.Game.Rulesets.Edit.Checks.Components;

namespace osu.Game.Rulesets.Edit.Checks
{
    public class CheckGenreLanguageTags : ICheck
    {
        public CheckMetadata Metadata { get; } = new CheckMetadata(CheckCategory.Metadata, "Missing genre or language in tags");

        public IEnumerable<IssueTemplate> PossibleTemplates => new IssueTemplate[]
        {
            new IssueTemplateMissingGenreOrLanguage(this)
        };

        public readonly struct TagCategory
        {
            public string Name { get; }
            private string[][] tagCombinations { get; }

            public TagCategory(string name, string[][] tagCombinations)
            {
                Name = name;
                this.tagCombinations = tagCombinations;
            }

            public string GetExamples()
            {
                var exampleTags = tagCombinations.Take(3).Select(tags => string.Join(" ", tags));

                return $"\"{string.Join("\", \"", exampleTags)}\"";
            }

            public bool ExistsIn(IEnumerable<string> tags)
            {
                return tagCombinations.Any(tagCombination =>
                    tagCombination.All(tagInCombination =>
                        tags.Any(tag =>
                            tag.ToLower().Contains(tagInCombination.ToLower())
                        )
                    )
                );
            }
        }

        private static readonly TagCategory genre_tags = new TagCategory("genre", new[]
        {
            new[] { "Video", "Game" },
            new[] { "Anime" },
            new[] { "Rock" },
            new[] { "Pop" },
            new[] { "Novelty" },
            new[] { "Hip", "Hop" },
            new[] { "Electronic" },
            new[] { "Metal" },
            new[] { "Classical" },
            new[] { "Folk" },
            new[] { "Jazz" }
        });

        private static readonly TagCategory language_tags = new TagCategory("language", new[]
        {
            new[] { "English" },
            new[] { "Chinese" },
            new[] { "French" },
            new[] { "German" },
            new[] { "Italian" },
            new[] { "Japanese" },
            new[] { "Korean" },
            new[] { "Spanish" },
            new[] { "Swedish" },
            new[] { "Russian" },
            new[] { "Polish" },
            new[] { "Instrumental" },

            // The following are not web languages, but if found, then the language would need to be "Other", so no point in warning.
            new[] { "Conlang" },
            new[] { "Hindi" },
            new[] { "Arabic" },
            new[] { "Portugese" },
            new[] { "Turkish" },
            new[] { "Vietnamese" },
            new[] { "Persian" },
            new[] { "Indonesian" },
            new[] { "Ukrainian" },
            new[] { "Romanian" },
            new[] { "Dutch" },
            new[] { "Thai" },
            new[] { "Greek" },
            new[] { "Somali" },
            new[] { "Malay" },
            new[] { "Hungarian" },
            new[] { "Czech" },
            new[] { "Norwegian" },
            new[] { "Finnish" },
            new[] { "Danish" },
            new[] { "Latvia" },
            new[] { "Lithuanian" },
            new[] { "Estonian" },
            new[] { "Punjabi" },
            new[] { "Bengali" }
        });

        public IEnumerable<Issue> Run(BeatmapVerifierContext context)
        {
            string[] tags = context.Beatmap.BeatmapInfo.Metadata.Tags.Split(" ");

            if (!genre_tags.ExistsIn(tags))
                yield return new IssueTemplateMissingGenreOrLanguage(this).Create(genre_tags);

            if (!language_tags.ExistsIn(tags))
                yield return new IssueTemplateMissingGenreOrLanguage(this).Create(language_tags);
        }

        public class IssueTemplateMissingGenreOrLanguage : IssueTemplate
        {
            public IssueTemplateMissingGenreOrLanguage(ICheck check)
                : base(check, IssueType.Problem, "Missing {0} tag (e.g. {1}).")
            {
            }

            public Issue Create(TagCategory tagCategory)
            {
                return new Issue(this, tagCategory.Name, tagCategory.GetExamples());
            }
        }
    }
}
