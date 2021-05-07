// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Edit.Checks.Components;

namespace osu.Game.Rulesets.Edit.Checks
{
    public class CheckBeatmapInfoConsistency : ICheck
    {
        public virtual CheckMetadata Metadata { get; } = new CheckMetadata(CheckCategory.Metadata, "Inconsistent beatmap options");

        public virtual IEnumerable<IssueTemplate> PossibleTemplates => new IssueTemplate[]
        {
            new IssueTemplateInconsistentFieldWarning(this)
        };

        protected readonly struct InfoField
        {
            public readonly string Name;
            public readonly Func<BeatmapInfo, object> Get;
            public readonly Func<IWorkingBeatmap, bool> Condition;

            public InfoField(string name, Func<BeatmapInfo, object> get, Func<IWorkingBeatmap, bool> condition = null)
            {
                Name = name;
                Get = get;
                Condition = condition;
            }

            public override string ToString() => Name;
        }

        public virtual IEnumerable<Issue> Run(IBeatmap playableBeatmap, WorkingBeatmapSet workingBeatmapSet)
        {
            var fields = new List<InfoField>
            {
                // TODO: In stable the countdown only matters if there's enough time to show it. Unsure if the same will apply to lazer.
                new InfoField("countdown", info => info.Countdown),
                new InfoField("epilepsy warning", info => info.EpilepsyWarning, condition: hasDrawableStoryboard),
                new InfoField("widescreen support", info => info.WidescreenStoryboard, condition: hasDrawableStoryboard),
                new InfoField("letterboxing", info => info.LetterboxInBreaks, condition: hasBreaks),
                new InfoField("audio lead-in", info => info.AudioLeadIn)
            };

            foreach (var issue in GetIssuesFromFields(playableBeatmap, workingBeatmapSet, fields))
                yield return issue;
        }

        /// <summary>
        /// Returns whether there is a storyboard with elements drawn. This includes videos.
        /// </summary>
        private bool hasDrawableStoryboard(IWorkingBeatmap workingBeatmap) => workingBeatmap.Storyboard.HasDrawable;

        private bool hasBreaks(IWorkingBeatmap workingBeatmap) => workingBeatmap.Beatmap.Breaks.Any();

        protected IEnumerable<Issue> GetIssuesFromFields(IBeatmap playableBeatmap, WorkingBeatmapSet workingBeatmapSet, List<InfoField> fields)
        {
            var curInfo = playableBeatmap.BeatmapInfo;

            foreach (var otherInfo in playableBeatmap.BeatmapInfo.BeatmapSet.Beatmaps)
            {
                if (curInfo == otherInfo)
                    // Don't compare with the current beatmap.
                    continue;

                foreach (var field in fields)
                {
                    if (field.Condition != null)
                    {
                        // The field must apply to both maps, else an inconsistency doesn't matter.
                        bool appliesToCurrent = field.Condition(workingBeatmapSet.GetWorkingBeatmap(curInfo));
                        bool appliesToOther = field.Condition(workingBeatmapSet.GetWorkingBeatmap(otherInfo));
                        if (!appliesToCurrent || !appliesToOther)
                            continue;
                    }

                    foreach (var issue in GetIssuesFromField(field, curInfo, otherInfo))
                        yield return issue;
                }
            }
        }

        protected virtual IEnumerable<Issue> GetIssuesFromField(InfoField field, BeatmapInfo info, BeatmapInfo otherInfo)
        {
            var content = field.Get(info)?.ToString();
            var otherContent = field.Get(otherInfo)?.ToString();
            if (content != otherContent)
                yield return new IssueTemplateInconsistentFieldWarning(this).Create(field.Name, content, otherInfo.Version, otherContent);
        }

        public abstract class IssueTemplateInconsistentField : IssueTemplate
        {
            protected IssueTemplateInconsistentField(ICheck check, IssueType issueType)
                : base(check, issueType, "Inconsistent {0} (\"{1}\") with [{2}] (\"{3}\").")
            {
            }

            public Issue Create(string fieldName, string fieldContent, string diffName, string otherFieldContent)
            {
                return new Issue(this, fieldName, fieldContent, diffName, otherFieldContent);
            }
        }

        public class IssueTemplateInconsistentFieldWarning : IssueTemplateInconsistentField
        {
            public IssueTemplateInconsistentFieldWarning(ICheck check)
                : base(check, IssueType.Warning)
            {
            }
        }
    }
}
