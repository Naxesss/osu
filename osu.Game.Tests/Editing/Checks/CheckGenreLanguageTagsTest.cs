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
    public class CheckGenreLanguageTagsTest
    {
        private CheckGenreLanguageTags check;

        [SetUp]
        public void Setup()
        {
            check = new CheckGenreLanguageTags();
        }

        [Test]
        public void TestHasGenreAndLanguage()
        {
            assertOk("Japanese Rock");
        }

        [Test]
        public void TestHasGenreAndRareLanguage()
        {
            assertOk("Norwegian Jazz");
        }

        [Test]
        public void TestHasTwoWordGenreAndLanguage()
        {
            assertOk("Hip English Hop");
        }

        [Test]
        public void TestMissingGenre()
        {
            assertMissing("abc English def", keywords: new[] { "genre" });
        }

        [Test]
        public void TestMissingLanguage()
        {
            assertMissing("abc Pop def", keywords: new[] { "language" });
        }

        [Test]
        public void TestNoTags()
        {
            assertMissing("", keywords: new[] { "genre", "language" });
        }

        private void assertOk(string tags)
        {
            Assert.That(check.Run(getContext(tags)), Is.Empty);
        }

        private void assertMissing(string tags, IReadOnlyCollection<string> keywords)
        {
            var issues = check.Run(getContext(tags)).ToList();

            Assert.That(issues, Has.Count.EqualTo(keywords.Count));
            Assert.That(issues.All(issue => issue.Template is CheckGenreLanguageTags.IssueTemplateMissingGenreOrLanguage));

            foreach (string keyword in keywords)
                Assert.That(issues.Any(issue => issue.ToString().Contains(keyword)));
        }

        private BeatmapVerifierContext getContext(string tags)
        {
            var beatmap = new Beatmap
            {
                BeatmapInfo = new BeatmapInfo
                {
                    Metadata = new BeatmapMetadata
                    {
                        Tags = tags
                    }
                }
            };

            return new BeatmapVerifierContext(beatmap, new TestWorkingBeatmap(beatmap));
        }
    }
}
