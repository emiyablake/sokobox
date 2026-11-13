using System.Collections.Generic;
using NUnit.Framework;
using Sokobox.Core;

namespace Sokobox.Tests
{
    /// <summary>
    /// Rules coverage for the pure core. These run without entering Play
    /// mode and encode the design decisions locked in GDD-dev — especially
    /// the locked-box rule, which the web prototype never implemented.
    /// </summary>
    public class SokobanGridTests
    {
        private static SokobanGrid GridFrom(params string[] map)
        {
            return new SokobanGrid(LevelParser.Parse("test", new List<string>(map)));
        }

        [Test]
        public void MoveIntoWall_IsBlocked()
        {
            var grid = GridFrom(
                "#####",
                "#@  #",
                "#####");

            var result = grid.Move(Direction.Left);

            Assert.AreEqual(MoveResultType.Blocked, result.Type);
            Assert.AreEqual(new GridPosition(1, 1), grid.Player);
            Assert.AreEqual(0, grid.Moves);
        }

        [Test]
        public void PushBox_IntoFreeCell_MovesBoth()
        {
            var grid = GridFrom(
                "######",
                "#@$ .#",
                "######");

            var result = grid.Move(Direction.Right);

            Assert.AreEqual(MoveResultType.Pushed, result.Type);
            Assert.AreEqual(new GridPosition(2, 1), grid.Player);
            Assert.AreEqual(new GridPosition(3, 1), grid.Boxes[0]);
            Assert.AreEqual(1, grid.Moves);
        }

        [Test]
        public void PushBox_IntoWall_IsBlocked()
        {
            var grid = GridFrom(
                "#####",
                "#@$##",
                "#####");

            var result = grid.Move(Direction.Right);

            Assert.AreEqual(MoveResultType.Blocked, result.Type);
            Assert.AreEqual(new GridPosition(1, 1), grid.Player);
            Assert.AreEqual(new GridPosition(2, 1), grid.Boxes[0]);
            Assert.AreEqual(0, grid.Moves);
        }

        [Test]
        public void BoxOnTarget_IsLocked_CannotBePushedOff()
        {
            var grid = GridFrom(
                "#######",
                "#@$  .#",
                "#######");

            // Park the box on the target.
            grid.Move(Direction.Right); // box -> (3,1)
            grid.Move(Direction.Right); // box -> (4,1)
            grid.Move(Direction.Right); // box -> (5,1) = target
            Assert.IsTrue(grid.IsComplete());

            // The lock: pushing the settled box must fail, and the box stays.
            var result = grid.Move(Direction.Right);

            Assert.AreEqual(MoveResultType.Blocked, result.Type);
            Assert.AreEqual(new GridPosition(4, 1), grid.Player);
            Assert.AreEqual(new GridPosition(5, 1), grid.Boxes[0]);
        }

        [Test]
        public void Move_WithoutBox_IncrementsCounter()
        {
            var grid = GridFrom(
                "#####",
                "#@  #",
                "#####");

            grid.Move(Direction.Right);
            grid.Move(Direction.Right);
            grid.Move(Direction.Up); // into the wall: must not count

            Assert.AreEqual(2, grid.Moves);
        }
    }
}
