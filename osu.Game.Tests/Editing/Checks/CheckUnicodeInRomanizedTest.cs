// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Edit.Checks;

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
                    Version = "Insane ?!()"
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
                    Version = "【Insanə】"
                }
            }, keywords: new[] { "アーティスト", "曲名", "【ə】" });
        }

        private void assertOk(IBeatmap beatmap)
        {
            Assert.That(check.Run(beatmap, null), Is.Empty);
        }

        private void assertUnicodeInRomanized(IBeatmap beatmap, IReadOnlyCollection<string> keywords)
        {
            var issues = check.Run(beatmap, null).ToList();

            Assert.That(issues, Has.Count.EqualTo(keywords.Count));
            Assert.That(issues.All(issue => issue.Template is CheckUnicodeInRomanized.IssueTemplateUnicodeInRomanizedField));

            foreach (var keyword in keywords)
                Assert.That(issues.Any(issue => issue.ToString().Contains(keyword)));
        }
    }
}
