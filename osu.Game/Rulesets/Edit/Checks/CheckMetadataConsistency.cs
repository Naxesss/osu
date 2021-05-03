// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Edit.Checks.Components;

namespace osu.Game.Rulesets.Edit.Checks
{
    public class CheckMetadataConsistency : CheckBeatmapInfoConsistency
    {
        public override CheckMetadata Metadata { get; } = new CheckMetadata(CheckCategory.Metadata, "Inconsistent metadata");

        public override IEnumerable<IssueTemplate> PossibleTemplates => new IssueTemplate[]
        {
            new IssueTemplateInconsistentFieldProblem(this)
        };

        public override IEnumerable<Issue> Run(IBeatmap playableBeatmap, WorkingBeatmapSet workingBeatmapSet)
        {
            // Background file and author fields may differ.
            // TODO: Author could not previously differ. I'm guessing Guest Difficulties will change that?
            var fields = new List<InfoField>
            {
                new InfoField("artist", info => info.Metadata.Artist),
                new InfoField("title", info => info.Metadata.Title),
                new InfoField("unicode artist", info => info.Metadata.ArtistUnicode),
                new InfoField("unicode title", info => info.Metadata.TitleUnicode),
                new InfoField("source", info => info.Metadata.Source),
                new InfoField("tags", info => info.Metadata.Tags),
                new InfoField("preview time", info => info.Metadata.PreviewTime),
                new InfoField("audio file", info => info.Metadata.AudioFile)
            };

            foreach (var issue in GetIssuesFromFields(playableBeatmap, workingBeatmapSet, fields))
                yield return issue;
        }

        protected override IEnumerable<Issue> GetIssuesFromField(InfoField field, BeatmapInfo info, BeatmapInfo otherInfo)
        {
            var content = field.Get(info)?.ToString();
            var otherContent = field.Get(otherInfo)?.ToString();
            if (content != otherContent)
                yield return new IssueTemplateInconsistentFieldProblem(this).Create(field.Name, content, otherInfo.Version, otherContent);
        }

        public class IssueTemplateInconsistentFieldProblem : IssueTemplateInconsistentField
        {
            public IssueTemplateInconsistentFieldProblem(ICheck check)
                : base(check, IssueType.Problem)
            {
            }
        }
    }
}
