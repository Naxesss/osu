// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;

namespace osu.Game.Beatmaps
{
    /// <summary>
    /// A representation of all cached <see cref="WorkingBeatmap"/> instances belonging to the same beatmapset.
    /// </summary>
    public class WorkingBeatmapSet
    {
        public readonly List<IWorkingBeatmap> WorkingBeatmaps;

        public WorkingBeatmapSet(params IWorkingBeatmap[] workingBeatmaps)
        {
            WorkingBeatmaps = workingBeatmaps.ToList();
        }

        public WorkingBeatmapSet(WorkingBeatmap currentWorkingBeatmap, BeatmapManager beatmapManager)
        {
            WorkingBeatmaps = new List<IWorkingBeatmap>();

            foreach (var beatmapInfo in currentWorkingBeatmap.BeatmapSetInfo.Beatmaps)
                WorkingBeatmaps.Add(beatmapManager.GetWorkingBeatmap(beatmapInfo, currentWorkingBeatmap));
        }

        /// <summary>
        /// Returns the cached working beatmap in this set corresponding to the given beatmap info, if any, otherwise null.
        /// </summary>
        /// <param name="beatmapInfo">The beatmap info of the working beatmap.</param>
        public IWorkingBeatmap GetWorkingBeatmap(BeatmapInfo beatmapInfo)
        {
            return WorkingBeatmaps.SingleOrDefault(workingBeatmap => workingBeatmap.Beatmap.BeatmapInfo == beatmapInfo);
        }
    }
}
