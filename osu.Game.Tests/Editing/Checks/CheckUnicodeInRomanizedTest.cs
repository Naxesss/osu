// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Edit.Checks;
using osu.Game.Tests.Beatmaps;

namespace osu.Game.Tests.Editing.Checks
{
    [TestFixture]
    public class CheckUnicodeInRomanizedTest
    {
        private CheckUnicodeInRomanized check;

        [SetUp]
        public void Setup()
        {
            check = new CheckUnicodeInRomanized();
        }

        [Test]
        public void TestNoUnicodeInRomanized()
        {
            assertOk(new Beatmap
            {
                BeatmapInfo = new BeatmapInfo
                {
                    Metadata = new BeatmapMetadata
                    {
                        Artist = "Artist &|_-+",
                        Title = "Title /.^*",
                        ArtistUnicode = "アーティスト",
                        TitleUnicode = "曲名"
                    },
                    DifficultyName = "Insane ?!()"
                }
            });
        }

        [Test]
        public void TestUnicodeInRomanized()
        {
            assertUnicodeInRomanized(new Beatmap
            {
                BeatmapInfo = new BeatmapInfo
                {
                    Metadata = new BeatmapMetadata
                    {
                        Artist = "アーティスト",
                        Title = "曲名"
                    },
                    DifficultyName = "【Insanə】"
                }
            }, keywords: new[] { "アーティスト", "曲名", "【ə】" });
        }

        private void assertOk(IBeatmap beatmap)
        {
            Assert.That(check.Run(getContext(beatmap)), Is.Empty);
        }

        private void assertUnicodeInRomanized(IBeatmap beatmap, IReadOnlyCollection<string> keywords)
        {
            var issues = check.Run(getContext(beatmap)).ToList();

            Assert.That(issues, Has.Count.EqualTo(keywords.Count));
            Assert.That(issues.All(issue => issue.Template is CheckUnicodeInRomanized.IssueTemplateUnicodeInRomanizedField));

            foreach (string keyword in keywords)
                Assert.That(issues.Any(issue => issue.ToString().Contains(keyword)));
        }

        private BeatmapVerifierContext getContext(IBeatmap beatmap)
        {
            return new BeatmapVerifierContext(beatmap, new TestWorkingBeatmap(beatmap));
        }
    }
}
