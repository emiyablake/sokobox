using System.Collections.Generic;
using Sokobox.Core;
using UnityEngine;

namespace Sokobox.Adapters
{
    /// <summary>
    /// Renders a SokobanGrid as SpriteRenderers, one GameObject per cell.
    /// Deliberately dumb: it only knows how to Show() a fresh grid and
    /// Refresh() positions after a move — all rules live in the core.
    /// Grid-to-world: 1 unit per cell, row grows downward (negated Y),
    /// matching how the ASCII maps read.
    /// </summary>
    public sealed class BoardView : MonoBehaviour
    {
        // Sorting orders keep the paint sequence: goal under wall under box under player.
        private const int GoalOrder = 0;
        private const int WallOrder = 1;
        private const int BoxOrder = 2;
        private const int PlayerOrder = 3;

        private Sprite _wallSprite;
        private Sprite _boxSprite;
        private Sprite _goalSprite;
        private Sprite _playerIdleSprite;
        private Sprite _playerPushSprite;

        private Transform _root;
        private readonly List<SpriteRenderer> _boxRenderers = new List<SpriteRenderer>();
        private SpriteRenderer _playerRenderer;

        public void Configure(Sprite wall, Sprite box, Sprite goal, Sprite playerIdle, Sprite playerPush)
        {
            _wallSprite = wall;
            _boxSprite = box;
            _goalSprite = goal;
            _playerIdleSprite = playerIdle;
            _playerPushSprite = playerPush;
        }

        public void Show(SokobanGrid grid)
        {
            if (_root != null)
            {
                Destroy(_root.gameObject);
            }

            _root = new GameObject("Board").transform;
            _root.SetParent(transform, false);
            _boxRenderers.Clear();

            foreach (var wall in grid.Level.Walls)
            {
                Spawn(_wallSprite, wall, WallOrder);
            }

            foreach (var target in grid.Level.Targets)
            {
                Spawn(_goalSprite, target, GoalOrder);
            }

            for (var i = 0; i < grid.Boxes.Count; i++)
            {
                _boxRenderers.Add(Spawn(_boxSprite, grid.Boxes[i], BoxOrder));
            }

            _playerRenderer = Spawn(_playerIdleSprite, grid.Player, PlayerOrder);
            FitCamera(grid.Level);
        }

        public void Refresh(SokobanGrid grid, bool pushing)
        {
            for (var i = 0; i < _boxRenderers.Count; i++)
            {
                _boxRenderers[i].transform.position = CellToWorld(grid.Boxes[i]);
            }

            _playerRenderer.transform.position = CellToWorld(grid.Player);
            _playerRenderer.sprite = pushing ? _playerPushSprite : _playerIdleSprite;
        }

        private SpriteRenderer Spawn(Sprite sprite, GridPosition cell, int sortingOrder)
        {
            var go = new GameObject(sprite.name);
            go.transform.SetParent(_root, false);
            go.transform.position = CellToWorld(cell);

            var renderer = go.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.sortingOrder = sortingOrder;
            return renderer;
        }

        private static Vector3 CellToWorld(GridPosition cell)
        {
            return new Vector3(cell.Col, -cell.Row, 0f);
        }

        /// <summary>
        /// Frames the whole board on the main camera regardless of level size.
        /// Grows the orthographic size when the level is wider than the
        /// viewport so nothing gets cropped on 16:9.
        /// </summary>
        private void FitCamera(LevelData level)
        {
            var camera = Camera.main;
            if (camera == null)
            {
                Debug.LogWarning("[Sokobox] No Main Camera tagged in the scene — board framing skipped.");
                return;
            }

            var centerX = (level.Width - 1) / 2f;
            var centerY = -(level.Height - 1) / 2f;
            camera.transform.position = new Vector3(centerX, centerY, -10f);

            var halfHeight = level.Height / 2f;
            var halfWidth = (level.Width / 2f) / camera.aspect;
            camera.orthographicSize = Mathf.Max(halfHeight, halfWidth) + 0.5f;
        }
    }
}
