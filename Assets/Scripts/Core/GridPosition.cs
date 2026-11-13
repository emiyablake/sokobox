using System;

namespace Sokobox.Core
{
    /// <summary>
    /// A cell coordinate on the level grid. Rows grow downward so that
    /// (0,0) is the top-left corner of the ASCII level files — this keeps
    /// the parser a straight copy of the web version's loop.
    /// </summary>
    public readonly struct GridPosition : IEquatable<GridPosition>
    {
        public readonly int Col;
        public readonly int Row;

        public GridPosition(int col, int row)
        {
            Col = col;
            Row = row;
        }

        public bool Equals(GridPosition other) => Col == other.Col && Row == other.Row;

        public override bool Equals(object obj) => obj is GridPosition other && Equals(other);

        public override int GetHashCode() => HashCode.Combine(Col, Row);

        public override string ToString() => $"({Col}, {Row})";
    }
}
