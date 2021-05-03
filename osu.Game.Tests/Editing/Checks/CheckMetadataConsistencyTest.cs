// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Edit.Checks;
using osu.Game.Rulesets.Objects;
using osu.Game.Users;

namespace osu.Game.Tests.Editing.Checks
{
    [TestFixture]
    public class CheckMetadataConsistencyTest
    {
        private CheckMetadataConsistency check;

        [SetUp]
        public void Setup()
        {
            check = new CheckMetadataConsistency();
        }

        [Test]
        public void TestConsistent()
        {
            var metadata1 = new BeatmapMetadata
            {
                Artist = "1",
                Title = "2",
                ArtistUnicode = "3",
                TitleUnicode = "4",
                AudioFile = "5",
                Author = new User { Username = "6" },
                BackgroundFile = "7",
                PreviewTime = 8,
                Source = "9",
                Tags = "10"
            };

            var metadata2 = new BeatmapMetadata
            {
                Artist = "1",
                Title = "2",
                ArtistUnicode = "3",
                TitleUnicode = "4",
                AudioFile = "5",
                Author = new User { Username = "Different" },
                BackgroundFile = "Different",
                PreviewTime = 8,
                Source = "9",
                Tags = "10"
            };

            assertOk(getPlayableBeatmap(metadata1, metadata2));
        }

        [Test]
        public void TestInconsistentArtistTitle()
        {
            var metadata1 = new BeatmapMetadata
            {
                Artist = "1",
                Title = "2",
                ArtistUnicode = "3",
                TitleUnicode = "4"
            };
            var metadata2 = new BeatmapMetadata
            {
                Artist = "Different",
                Title = "Different",
                ArtistUnicode = "Different",
                TitleUnicode = "Different"
            };

            assertInconsistent(getPlayableBeatmap(metadata1, metadata2), new[]
            {
                "artist", "title", "unicode artist", "unicode title"
            });
        }

        [Test]
        public void TestInconsistentTagsSourceAudio()
        {
            var metadata1 = new BeatmapMetadata { Tags = "1", Source = "2", AudioFile = "3" };
            var metadata2 = new BeatmapMetadata { Tags = "Different", Source = "Different", AudioFile = "Different" };

            assertInconsistent(getPlayableBeatmap(metadata1, metadata2), new[] { "tags", "source", "audio file" });
        }

        [Test]
        public void TestInconsistentPreviewTime()
        {
            var metadata1 = new BeatmapMetadata { PreviewTime = 100 };
            var metadata2 = new BeatmapMetadata { PreviewTime = 200 };

            assertInconsistent(getPlayableBeatmap(metadata1, metadata2), new[] { "preview time" });
        }

        private IBeatmap getPlayableBeatmap(params BeatmapMetadata[] metadataArr)
        {
            var beatmaps = metadataArr.Select(metadata => new BeatmapInfo { Metadata = metadata }).ToList();

            return new Beatmap<HitObject>
            {
                BeatmapInfo = new BeatmapInfo
                {
                    Metadata = metadataArr[0],
                    BeatmapSet = new BeatmapSetInfo { Beatmaps = beatmaps }
                }
            };
        }

        private void assertOk(IBeatmap beatmap)
        {
            Assert.That(check.Run(beatmap, null), Is.Empty);
        }

        private void assertInconsistent(IBeatmap beatmap, IReadOnlyCollection<string> keywords)
        {
            var issues = check.Run(beatmap, null).ToList();

            Assert.That(issues, Has.Count.EqualTo(keywords.Count));
            Assert.That(issues.All(issue => issue.Template is CheckMetadataConsistency.IssueTemplateInconsistentFieldProblem));

            foreach (var keyword in keywords)
                Assert.That(issues.Any(issue => issue.ToString().Contains(keyword)));
        }
    }
}
