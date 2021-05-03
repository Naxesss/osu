// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Edit.Checks.Components;

namespace osu.Game.Rulesets.Edit.Checks
{
    public class CheckUnicodeInRomanized : ICheck
    {
        public CheckMetadata Metadata { get; } = new CheckMetadata(CheckCategory.Metadata, "Unicode characters in romanized fields");

        public IEnumerable<IssueTemplate> PossibleTemplates => new IssueTemplate[]
        {
            new IssueTemplateUnicodeInRomanizedField(this)
        };

        private readonly struct RomanizedField
        {
            public readonly string Name;
            public readonly Func<BeatmapInfo, string> Get;

            public RomanizedField(string name, Func<BeatmapInfo, string> get)
            {
                Name = name;
                Get = get;
            }
        }

        public IEnumerable<Issue> Run(IBeatmap playableBeatmap, WorkingBeatmapSet workingBeatmapSet)
        {
            var fields = new List<RomanizedField>
            {
                new RomanizedField("artist", info => info.Metadata.Artist),
                new RomanizedField("title", info => info.Metadata.Title),
                new RomanizedField("difficulty name", info => info.Version)
            };

            foreach (var field in fields)
            {
                var content = field.Get(playableBeatmap.BeatmapInfo);
                if (content.Any(isUnicode))
                    yield return new IssueTemplateUnicodeInRomanizedField(this).Create(field.Name, content, getUnicodeCharacters(content));
            }
        }

        private bool isUnicode(char c) => c > 127;

        private string getUnicodeCharacters(string text) => string.Concat(text.Where(isUnicode));

        public class IssueTemplateUnicodeInRomanizedField : IssueTemplate
        {
            public IssueTemplateUnicodeInRomanizedField(ICheck check)
                : base(check, IssueType.Problem, "The {0} field (\"{1}\") contains unicode characters (\"{2}\").")
            {
            }

            public Issue Create(string name, string content, string unicodeCharacters)
            {
                return new Issue(this, name, content, unicodeCharacters);
            }
        }
    }
}
