namespace Sokobox.Core
{
    public enum Direction
    {
        Up,
        Down,
        Left,
        Right,
    }

    public static class DirectionExtensions
    {
        /// <summary>Grid delta for a direction, matching the ASCII maps (row grows downward).</summary>
        public static (int dCol, int dRow) Delta(this Direction direction)
        {
            switch (direction)
            {
                case Direction.Up: return (0, -1);
                case Direction.Down: return (0, 1);
                case Direction.Left: return (-1, 0);
                case Direction.Right: return (1, 0);
                default: return (0, 0);
            }
        }
    }
}
