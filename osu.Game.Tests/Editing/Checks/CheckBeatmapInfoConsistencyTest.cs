// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using Moq;
using NUnit.Framework;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.Timing;
using osu.Game.Rulesets.Edit.Checks;
using osu.Game.Rulesets.Objects;
using osu.Game.Storyboards;
using osu.Game.Tests.Beatmaps;

namespace osu.Game.Tests.Editing.Checks
{
    [TestFixture]
    public class CheckBeatmapInfoConsistencyTest
    {
        private CheckBeatmapInfoConsistency check;

        [SetUp]
        public void Setup()
        {
            check = new CheckBeatmapInfoConsistency();
        }

        [Test]
        public void TestConsistent()
        {
            var info1 = new BeatmapInfo
            {
                AudioLeadIn = 1,
                Countdown = true,
                EpilepsyWarning = false,
                WidescreenStoryboard = true,
                LetterboxInBreaks = false,

                Version = "Hard",
                Length = 120
            };

            var info2 = new BeatmapInfo
            {
                AudioLeadIn = 1,
                Countdown = true,
                EpilepsyWarning = false,
                WidescreenStoryboard = true,
                LetterboxInBreaks = false,

                Version = "Expert",
                Length = 150
            };

            var breaks = new List<BreakPeriod> { new BreakPeriod(100, 300) };
            var workingBeatmap1 = new TestWorkingBeatmap(new Beatmap { BeatmapInfo = info1, Breaks = breaks }, getDrawableStoryboard());
            var workingBeatmap2 = new TestWorkingBeatmap(new Beatmap { BeatmapInfo = info2, Breaks = breaks }, getDrawableStoryboard());
            var workingBeatmapSet = new WorkingBeatmapSet(workingBeatmap1, workingBeatmap2);

            assertOk(getPlayableBeatmap(info1, info2), workingBeatmapSet);
        }

        [Test]
        public void TestInconsistentAudioLeadIn()
        {
            var info1 = new BeatmapInfo { AudioLeadIn = 100 };
            var info2 = new BeatmapInfo { AudioLeadIn = 200 };

            var workingBeatmap1 = new TestWorkingBeatmap(new Beatmap { BeatmapInfo = info1 });
            var workingBeatmap2 = new TestWorkingBeatmap(new Beatmap { BeatmapInfo = info2 });
            var workingBeatmapSet = new WorkingBeatmapSet(workingBeatmap1, workingBeatmap2);

            assertInconsistent(getPlayableBeatmap(info1, info2), workingBeatmapSet, new[] { "audio lead-in" });
        }

        [Test]
        public void TestInconsistentStoryboardOptionsWithStoryboard()
        {
            var info1 = new BeatmapInfo { EpilepsyWarning = true, WidescreenStoryboard = true };
            var info2 = new BeatmapInfo { EpilepsyWarning = false, WidescreenStoryboard = false };

            var workingBeatmap1 = new TestWorkingBeatmap(new Beatmap { BeatmapInfo = info1 }, getDrawableStoryboard());
            var workingBeatmap2 = new TestWorkingBeatmap(new Beatmap { BeatmapInfo = info2 }, getDrawableStoryboard());
            var workingBeatmapSet = new WorkingBeatmapSet(workingBeatmap1, workingBeatmap2);

            assertInconsistent(getPlayableBeatmap(info1, info2), workingBeatmapSet, new[] { "epilepsy warning", "widescreen" });
        }

        [Test]
        public void TestInconsistentStoryboardOptionsWithoutStoryboard()
        {
            assertInconsistentStoryboardOptionsOk(getDrawableStoryboard(), null);
            assertInconsistentStoryboardOptionsOk(null, getDrawableStoryboard());
            assertInconsistentStoryboardOptionsOk(null, null);
        }

        [Test]
        public void TestInconsistentLetterboxingWithBreaks()
        {
            var info1 = new BeatmapInfo { LetterboxInBreaks = true };
            var info2 = new BeatmapInfo { LetterboxInBreaks = false };

            var breaks = new List<BreakPeriod> { new BreakPeriod(100, 300) };
            var workingBeatmap1 = new TestWorkingBeatmap(new Beatmap { BeatmapInfo = info1, Breaks = breaks });
            var workingBeatmap2 = new TestWorkingBeatmap(new Beatmap { BeatmapInfo = info2, Breaks = breaks });
            var workingBeatmapSet = new WorkingBeatmapSet(workingBeatmap1, workingBeatmap2);

            assertInconsistent(getPlayableBeatmap(info1, info2), workingBeatmapSet, new[] { "letterbox" });
        }

        [Test]
        public void TestInconsistentLetterboxingWithoutBreaks()
        {
            assertInconsistentLetterboxingOk(
                new List<BreakPeriod> { new BreakPeriod(100, 300) },
                new List<BreakPeriod>()
            );
            assertInconsistentLetterboxingOk(
                new List<BreakPeriod>(),
                new List<BreakPeriod> { new BreakPeriod(100, 300) }
            );
            assertInconsistentLetterboxingOk(
                new List<BreakPeriod>(),
                new List<BreakPeriod>()
            );
        }

        private void assertInconsistentStoryboardOptionsOk(Storyboard storyboard1, Storyboard storyboard2)
        {
            var info1 = new BeatmapInfo { EpilepsyWarning = true, WidescreenStoryboard = true };
            var info2 = new BeatmapInfo { EpilepsyWarning = false, WidescreenStoryboard = false };

            var workingBeatmap1 = new TestWorkingBeatmap(new Beatmap { BeatmapInfo = info1 }, storyboard1);
            var workingBeatmap2 = new TestWorkingBeatmap(new Beatmap { BeatmapInfo = info2 }, storyboard2);
            var workingBeatmapSet = new WorkingBeatmapSet(workingBeatmap1, workingBeatmap2);

            assertOk(getPlayableBeatmap(info1, info2), workingBeatmapSet);
        }

        private void assertInconsistentLetterboxingOk(List<BreakPeriod> breaks1, List<BreakPeriod> breaks2)
        {
            var info1 = new BeatmapInfo { LetterboxInBreaks = true };
            var info2 = new BeatmapInfo { LetterboxInBreaks = false };

            var workingBeatmap1 = new TestWorkingBeatmap(new Beatmap { BeatmapInfo = info1, Breaks = breaks1 });
            var workingBeatmap2 = new TestWorkingBeatmap(new Beatmap { BeatmapInfo = info2, Breaks = breaks2 });
            var workingBeatmapSet = new WorkingBeatmapSet(workingBeatmap1, workingBeatmap2);

            assertOk(getPlayableBeatmap(info1, info2), workingBeatmapSet);
        }

        private IBeatmap getPlayableBeatmap(params BeatmapInfo[] infoArr)
        {
            // Comparing is done against the current beatmap, which is the one at index 0 in this case.
            infoArr[0].BeatmapSet = new BeatmapSetInfo { Beatmaps = infoArr.ToList() };

            return new Beatmap<HitObject> { BeatmapInfo = infoArr[0] };
        }

        private void assertOk(IBeatmap beatmap, WorkingBeatmapSet workingBeatmapSet)
        {
            Assert.That(check.Run(beatmap, workingBeatmapSet), Is.Empty);
        }

        private void assertInconsistent(IBeatmap beatmap, WorkingBeatmapSet workingBeatmapSet, IReadOnlyCollection<string> keywords)
        {
            var issues = check.Run(beatmap, workingBeatmapSet).ToList();

            Assert.That(issues, Has.Count.EqualTo(keywords.Count));
            Assert.That(issues.All(issue => issue.Template is CheckBeatmapInfoConsistency.IssueTemplateInconsistentFieldWarning));

            foreach (var keyword in keywords)
                Assert.That(issues.Any(issue => issue.ToString().Contains(keyword)));
        }

        private Storyboard getDrawableStoryboard()
        {
            var storyboard = new Storyboard();
            var mockElement = new Mock<IStoryboardElement>();
            mockElement.SetupGet(e => e.IsDrawable).Returns(true);
            storyboard.Layers.First().Elements.Add(mockElement.Object);

            return storyboard;
        }
    }
}
