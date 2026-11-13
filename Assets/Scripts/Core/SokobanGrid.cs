using System.Collections.Generic;

namespace Sokobox.Core
{
    public enum MoveResultType
    {
        Moved,
        Pushed,
        Blocked,
    }

    /// <summary>Outcome of a single move attempt. Never throws — a blocked move is a result, not an error.</summary>
    public readonly struct MoveResult
    {
        public readonly MoveResultType Type;

        public MoveResult(MoveResultType type)
        {
            Type = type;
        }
    }

    /// <summary>
    /// The pure game rules of Sokoban, with no Unity dependency so the whole
    /// thing can be unit-tested without booting the engine. Mirrors the web
    /// prototype's movePlayer(), plus the design decisions locked in GDD-dev:
    /// boxes resting on targets are locked, and the move counter is a score
    /// metric (there is deliberately no move limit anywhere in here).
    /// </summary>
    public sealed class SokobanGrid
    {
        public LevelData Level { get; }
        public GridPosition Player { get; private set; }
        public int Moves { get; private set; }

        private readonly HashSet<GridPosition> _targets;
        private readonly List<GridPosition> _boxes;

        public SokobanGrid(LevelData level)
        {
            Level = level;
            Player = level.PlayerStart;
            _targets = level.Targets;
            _boxes = new List<GridPosition>(level.Boxes);
        }

        /// <summary>Current box positions, index-stable within a level so views can track individual boxes.</summary>
        public IReadOnlyList<GridPosition> Boxes => _boxes;

        public MoveResult Move(Direction direction)
        {
            var (dCol, dRow) = direction.Delta();
            var destination = new GridPosition(Player.Col + dCol, Player.Row + dRow);

            if (Level.Walls.Contains(destination))
            {
                return new MoveResult(MoveResultType.Blocked);
            }

            var boxIndex = _boxes.IndexOf(destination);
            var pushed = false;

            if (boxIndex >= 0)
            {
                // Design decision (GDD-dev): a box on a target is locked and
                // can never be pushed off it.
                if (_targets.Contains(destination))
                {
                    return new MoveResult(MoveResultType.Blocked);
                }

                var boxDestination = new GridPosition(destination.Col + dCol, destination.Row + dRow);
                if (Level.Walls.Contains(boxDestination) || _boxes.Contains(boxDestination))
                {
                    return new MoveResult(MoveResultType.Blocked);
                }

                _boxes[boxIndex] = boxDestination;
                pushed = true;
            }

            Player = destination;
            Moves++;
            return new MoveResult(pushed ? MoveResultType.Pushed : MoveResultType.Moved);
        }

        /// <summary>Win check: every target covered by a box.</summary>
        public bool IsComplete()
        {
            foreach (var target in _targets)
            {
                if (!_boxes.Contains(target))
                {
                    return false;
                }
            }
            return true;
        }
    }
}
