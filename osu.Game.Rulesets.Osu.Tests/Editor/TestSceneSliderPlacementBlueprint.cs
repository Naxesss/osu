// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Framework.Utils;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.Osu.Edit.Blueprints.Sliders;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Osu.Objects.Drawables;
using osu.Game.Tests.Visual;
using osuTK;
using osuTK.Input;

namespace osu.Game.Rulesets.Osu.Tests.Editor
{
    public class TestSceneSliderPlacementBlueprint : PlacementBlueprintTestScene
    {
        [SetUp]
        public void Setup() => Schedule(() =>
        {
            HitObjectContainer.Clear();
            ResetPlacement();
        });

        [Test]
        public void TestBeginPlacementWithoutFinishing()
        {
            addMovementStep(new Vector2(200));
            addClickStep(MouseButton.Left);

            assertPlaced(false);
        }

        [Test]
        public void TestPlaceWithoutMovingMouse()
        {
            addMovementStep(new Vector2(200));
            addClickStep(MouseButton.Left);
            addClickStep(MouseButton.Right);

            assertPlaced(true);
            assertLength(0);
            assertControlPointType(0, PathType.Linear);
        }

        [Test]
        public void TestPlaceWithMouseMovement()
        {
            addMovementStep(new Vector2(200));
            addClickStep(MouseButton.Left);

            addMovementStep(new Vector2(400, 200));
            addClickStep(MouseButton.Right);

            assertPlaced(true);
            assertLength(200);
            assertControlPointCount(2);
            assertControlPointType(0, PathType.Linear);
        }

        [Test]
        public void TestPlaceNormalControlPoint()
        {
            addMovementStep(new Vector2(200));
            addClickStep(MouseButton.Left);

            addMovementStep(new Vector2(300, 200));
            addClickStep(MouseButton.Left);

            addMovementStep(new Vector2(300));
            addClickStep(MouseButton.Right);

            assertPlaced(true);
            assertControlPointCount(3);
            assertControlPointPosition(1, new Vector2(100, 0));
            assertControlPointType(0, PathType.PerfectCurve);
        }

        [Test]
        public void TestPlaceTwoNormalControlPoints()
        {
            addMovementStep(new Vector2(200));
            addClickStep(MouseButton.Left);

            addMovementStep(new Vector2(300, 200));
            addClickStep(MouseButton.Left);

            addMovementStep(new Vector2(300));
            addClickStep(MouseButton.Left);

            addMovementStep(new Vector2(400, 300));
            addClickStep(MouseButton.Right);

            assertPlaced(true);
            assertControlPointCount(4);
            assertControlPointPosition(1, new Vector2(100, 0));
            assertControlPointPosition(2, new Vector2(100, 100));
            assertControlPointType(0, PathType.Bezier);
        }

        [Test]
        public void TestPlaceSegmentControlPoint()
        {
            addMovementStep(new Vector2(200));
            addClickStep(MouseButton.Left);

            addMovementStep(new Vector2(300, 200));
            addClickStep(MouseButton.Left);
            addClickStep(MouseButton.Left);

            addMovementStep(new Vector2(300));
            addClickStep(MouseButton.Right);

            assertPlaced(true);
            assertControlPointCount(3);
            assertControlPointPosition(1, new Vector2(100, 0));
            assertControlPointType(0, PathType.Linear);
            assertControlPointType(1, PathType.Linear);
        }

        [Test]
        public void TestMoveToPerfectCurveThenPlaceLinear()
        {
            addMovementStep(new Vector2(200));
            addClickStep(MouseButton.Left);

            addMovementStep(new Vector2(300, 200));
            addClickStep(MouseButton.Left);

            addMovementStep(new Vector2(300));
            addMovementStep(new Vector2(300, 200));
            addClickStep(MouseButton.Right);

            assertPlaced(true);
            assertControlPointCount(2);
            assertControlPointType(0, PathType.Linear);
            assertLength(100);
        }

        [Test]
        public void TestMoveToBezierThenPlacePerfectCurve()
        {
            addMovementStep(new Vector2(200));
            addClickStep(MouseButton.Left);

            addMovementStep(new Vector2(300, 200));
            addClickStep(MouseButton.Left);

            addMovementStep(new Vector2(300));
            addClickStep(MouseButton.Left);

            addMovementStep(new Vector2(400, 300));
            addMovementStep(new Vector2(300));
            addClickStep(MouseButton.Right);

            assertPlaced(true);
            assertControlPointCount(3);
            assertControlPointType(0, PathType.PerfectCurve);
        }

        [Test]
        public void TestMoveToFourthOrderBezierThenPlaceThirdOrderBezier()
        {
            addMovementStep(new Vector2(200));
            addClickStep(MouseButton.Left);

            addMovementStep(new Vector2(300, 200));
            addClickStep(MouseButton.Left);

            addMovementStep(new Vector2(300));
            addClickStep(MouseButton.Left);

            addMovementStep(new Vector2(400, 300));
            addClickStep(MouseButton.Left);

            addMovementStep(new Vector2(400));
            addMovementStep(new Vector2(400, 300));
            addClickStep(MouseButton.Right);

            assertPlaced(true);
            assertControlPointCount(4);
            assertControlPointType(0, PathType.Bezier);
        }

        [Test]
        public void TestPlaceLinearSegmentThenPlaceLinearSegment()
        {
            addMovementStep(new Vector2(200));
            addClickStep(MouseButton.Left);

            addMovementStep(new Vector2(300, 200));
            addClickStep(MouseButton.Left);
            addClickStep(MouseButton.Left);

            addMovementStep(new Vector2(300, 300));
            addClickStep(MouseButton.Right);

            assertPlaced(true);
            assertControlPointCount(3);
            assertControlPointPosition(1, new Vector2(100, 0));
            assertControlPointPosition(2, new Vector2(100));
            assertControlPointType(0, PathType.Linear);
            assertControlPointType(1, PathType.Linear);
        }

        [Test]
        public void TestPlaceLinearSegmentThenPlacePerfectCurveSegment()
        {
            addMovementStep(new Vector2(200));
            addClickStep(MouseButton.Left);

            addMovementStep(new Vector2(300, 200));
            addClickStep(MouseButton.Left);
            addClickStep(MouseButton.Left);

            addMovementStep(new Vector2(300, 300));
            addClickStep(MouseButton.Left);

            addMovementStep(new Vector2(400, 300));
            addClickStep(MouseButton.Right);

            assertPlaced(true);
            assertControlPointCount(4);
            assertControlPointPosition(1, new Vector2(100, 0));
            assertControlPointPosition(2, new Vector2(100));
            assertControlPointType(0, PathType.Linear);
            assertControlPointType(1, PathType.PerfectCurve);
        }

        [Test]
        public void TestPlacePerfectCurveSegmentThenPlacePerfectCurveSegment()
        {
            addMovementStep(new Vector2(200));
            addClickStep(MouseButton.Left);

            addMovementStep(new Vector2(300, 200));
            addClickStep(MouseButton.Left);

            addMovementStep(new Vector2(300, 300));
            addClickStep(MouseButton.Left);
            addClickStep(MouseButton.Left);

            addMovementStep(new Vector2(400, 300));
            addClickStep(MouseButton.Left);

            addMovementStep(new Vector2(400));
            addClickStep(MouseButton.Right);

            assertPlaced(true);
            assertControlPointCount(5);
            assertControlPointPosition(1, new Vector2(100, 0));
            assertControlPointPosition(2, new Vector2(100));
            assertControlPointPosition(3, new Vector2(200, 100));
            assertControlPointPosition(4, new Vector2(200));
            assertControlPointType(0, PathType.PerfectCurve);
            assertControlPointType(2, PathType.PerfectCurve);
        }

        [Test]
        public void TestBeginPlacementWithoutReleasingMouse()
        {
            addMovementStep(new Vector2(200));
            AddStep("press left button", () => InputManager.PressButton(MouseButton.Left));

            addMovementStep(new Vector2(400, 200));
            AddStep("release left button", () => InputManager.ReleaseButton(MouseButton.Left));

            addClickStep(MouseButton.Right);

            assertPlaced(true);
            assertLength(200);
            assertControlPointCount(2);
            assertControlPointType(0, PathType.Linear);
        }

        [Test]
        public void TestPlacePerfectCurveSegmentAlmostLinearlyExterior()
        {
            Vector2 startPosition = new Vector2(200);

            addMovementStep(startPosition);
            addClickStep(MouseButton.Left);

            addMovementStep(startPosition + new Vector2(300, 0));
            addClickStep(MouseButton.Left);

            addMovementStep(startPosition + new Vector2(150, 0.1f));
            addClickStep(MouseButton.Right);

            assertPlaced(true);
            assertControlPointCount(3);
            assertControlPointType(0, PathType.Bezier);
        }

        [Test]
        public void TestPlacePerfectCurveSegmentRecovery()
        {
            Vector2 startPosition = new Vector2(200);

            addMovementStep(startPosition);
            addClickStep(MouseButton.Left);

            addMovementStep(startPosition + new Vector2(300, 0));
            addClickStep(MouseButton.Left);

            addMovementStep(startPosition + new Vector2(150, 0.1f)); // Should convert to bezier
            addMovementStep(startPosition + new Vector2(400.0f, 50.0f)); // Should convert back to perfect
            addClickStep(MouseButton.Right);

            assertPlaced(true);
            assertControlPointCount(3);
            assertControlPointType(0, PathType.PerfectCurve);
        }

        [Test]
        public void TestPlacePerfectCurveSegmentLarge()
        {
            Vector2 startPosition = new Vector2(400);

            addMovementStep(startPosition);
            addClickStep(MouseButton.Left);

            addMovementStep(startPosition + new Vector2(240, 240));
            addClickStep(MouseButton.Left);

            // Playfield dimensions are 640 x 480.
            // So a 480 x 480 bounding box should be ok.
            addMovementStep(startPosition + new Vector2(-240, 240));
            addClickStep(MouseButton.Right);

            assertPlaced(true);
            assertControlPointCount(3);
            assertControlPointType(0, PathType.PerfectCurve);
        }

        [Test]
        public void TestPlacePerfectCurveSegmentTooLarge()
        {
            Vector2 startPosition = new Vector2(480, 200);

            addMovementStep(startPosition);
            addClickStep(MouseButton.Left);

            addMovementStep(startPosition + new Vector2(400, 400));
            addClickStep(MouseButton.Left);

            // Playfield dimensions are 640 x 480.
            // So an 800 * 800 bounding box area should not be ok.
            addMovementStep(startPosition + new Vector2(-400, 400));
            addClickStep(MouseButton.Right);

            assertPlaced(true);
            assertControlPointCount(3);
            assertControlPointType(0, PathType.Bezier);
        }

        [Test]
        public void TestPlacePerfectCurveSegmentCompleteArc()
        {
            addMovementStep(new Vector2(400));
            addClickStep(MouseButton.Left);

            addMovementStep(new Vector2(600, 400));
            addClickStep(MouseButton.Left);

            addMovementStep(new Vector2(400, 410));
            addClickStep(MouseButton.Right);

            assertPlaced(true);
            assertControlPointCount(3);
            assertControlPointType(0, PathType.PerfectCurve);
        }

        private void addMovementStep(Vector2 position) => AddStep($"move mouse to {position}", () => InputManager.MoveMouseTo(InputManager.ToScreenSpace(position)));

        private void addClickStep(MouseButton button)
        {
            AddStep($"click {button}", () => InputManager.Click(button));
        }

        private void assertPlaced(bool expected) => AddAssert($"slider {(expected ? "placed" : "not placed")}", () => (getSlider() != null) == expected);

        private void assertLength(double expected) => AddAssert($"slider length is {expected}", () => Precision.AlmostEquals(expected, getSlider().Distance, 1));

        private void assertControlPointCount(int expected) => AddAssert($"has {expected} control points", () => getSlider().Path.ControlPoints.Count == expected);

        private void assertControlPointType(int index, PathType type) => AddAssert($"control point {index} is {type}", () => getSlider().Path.ControlPoints[index].Type.Value == type);

        private void assertControlPointPosition(int index, Vector2 position) =>
            AddAssert($"control point {index} at {position}", () => Precision.AlmostEquals(position, getSlider().Path.ControlPoints[index].Position.Value, 1));

        private Slider getSlider() => HitObjectContainer.Count > 0 ? ((DrawableSlider)HitObjectContainer[0]).HitObject : null;

        protected override DrawableHitObject CreateHitObject(HitObject hitObject) => new DrawableSlider((Slider)hitObject);
        protected override PlacementBlueprint CreateBlueprint() => new SliderPlacementBlueprint();
    }
}
