using System.Collections.Generic;

namespace Sokobox.Core
{
    /// <summary>
    /// Immutable starting layout of one level. Mutable play state lives in
    /// SokobanGrid — this is just what the level file says before the player
    /// takes the first step.
    /// </summary>
    public sealed class LevelData
    {
        public string Name { get; }
        public int Width { get; }
        public int Height { get; }
        public GridPosition PlayerStart { get; }
        public HashSet<GridPosition> Walls { get; }
        public List<GridPosition> Boxes { get; }
        public HashSet<GridPosition> Targets { get; }

        public LevelData(
            string name,
            int width,
            int height,
            GridPosition playerStart,
            HashSet<GridPosition> walls,
            List<GridPosition> boxes,
            HashSet<GridPosition> targets)
        {
            Name = name;
            Width = width;
            Height = height;
            PlayerStart = playerStart;
            Walls = walls;
            Boxes = boxes;
            Targets = targets;
        }
    }
}
