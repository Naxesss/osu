// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Game.Rulesets.Edit.Checks.Components;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Types;

namespace osu.Game.Rulesets.Edit.Checks
{
    public class CheckConcurrentObjects : ICheck
    {
        // We guarantee that the objects are either treated as concurrent or unsnapped when near the same beat divisor.
        private const double ms_leniency = CheckUnsnappedObjects.UNSNAP_MS_THRESHOLD;

        public CheckMetadata Metadata { get; } = new CheckMetadata(CheckCategory.Compose, "Concurrent hitobjects");

        public IEnumerable<IssueTemplate> PossibleTemplates => new IssueTemplate[]
        {
            new IssueTemplateConcurrent(this),
            new IssueTemplateAlmostConcurrent(this)
        };

        public IEnumerable<Issue> Run(BeatmapVerifierContext context)
        {
            var hitObjects = context.Beatmap.HitObjects;

            for (int i = 0; i < hitObjects.Count - 1; ++i)
            {
                var hitobject = hitObjects[i];

                for (int j = i + 1; j < hitObjects.Count; ++j)
                {
                    var nextHitobject = hitObjects[j];

                    // Accounts for rulesets with hitobjects separated by columns, such as Mania.
                    // In these cases we only care about concurrent objects within the same column.
                    if ((hitobject as IHasColumn)?.Column != (nextHitobject as IHasColumn)?.Column)
                        continue;

                    // Two hitobjects cannot be concurrent without also being concurrent with all objects in between.
                    // So if the next object is not concurrent, then we know no future objects will be either.
                    if (!areConcurrent(hitobject, nextHitobject))
                    {
                        if (!areAlmostConcurrent(hitobject, nextHitobject))
                            break;

                        yield return new IssueTemplateAlmostConcurrent(this).Create(hitobject, nextHitobject);

                        // There could be more objects almost concurrent, so continue until that's no longer the case.
                        continue;
                    }

                    yield return new IssueTemplateConcurrent(this).Create(hitobject, nextHitobject);
                }
            }
        }

        private bool areConcurrent(HitObject hitobject, HitObject nextHitobject) => nextHitobject.StartTime <= hitobject.GetEndTime() + ms_leniency;
        private bool areAlmostConcurrent(HitObject hitobject, HitObject nextHitobject) => nextHitobject.StartTime <= hitobject.GetEndTime() + 10;

        private static string getFancyTypeString(HitObject hitobject, HitObject nextHitobject)
        {
            string typeName = hitobject.GetType().Name;
            string nextTypeName = nextHitobject.GetType().Name;

            return typeName == nextTypeName ? $"{typeName}s" : $"{typeName} and {nextTypeName}";
        }

        public class IssueTemplateConcurrent : IssueTemplate
        {
            public IssueTemplateConcurrent(ICheck check)
                : base(check, IssueType.Problem, "{0} are concurrent.")
            {
            }

            public Issue Create(HitObject hitobject, HitObject nextHitobject)
            {
                var hitobjects = new List<HitObject> { hitobject, nextHitobject };
                return new Issue(hitobjects, this, getFancyTypeString(hitobject, nextHitobject))
                {
                    Time = nextHitobject.StartTime
                };
            }
        }

        public class IssueTemplateAlmostConcurrent : IssueTemplate
        {
            public IssueTemplateAlmostConcurrent(ICheck check)
                : base(check, IssueType.Problem, "{0} are less than 10 ms apart ({1} ms).")
            {
            }

            public Issue Create(HitObject hitobject, HitObject nextHitobject)
            {
                var hitobjects = new List<HitObject> { hitobject, nextHitobject };
                return new Issue(hitobjects, this, getFancyTypeString(hitobject, nextHitobject), hitobject.GetEndTime() - nextHitobject.StartTime)
                {
                    Time = nextHitobject.StartTime
                };
            }
        }
    }
}
