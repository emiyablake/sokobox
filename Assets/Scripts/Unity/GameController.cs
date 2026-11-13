using System.Collections;
using System.Collections.Generic;
using Sokobox.Core;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Sokobox.Adapters
{
    /// <summary>
    /// Entry point: loads levels from Resources, owns the current SokobanGrid,
    /// translates Input System events into core Move() calls and forwards
    /// state to the BoardView. Keeps the same feel as the web prototype:
    /// instant grid movement, auto-advance after a short beat on completion.
    /// </summary>
    public sealed class GameController : MonoBehaviour
    {
        [Header("Sprites (from Art/Sprites)")]
        [SerializeField] private Sprite _wallSprite;
        [SerializeField] private Sprite _boxSprite;
        [SerializeField] private Sprite _goalSprite;
        [SerializeField] private Sprite _playerIdleSprite;
        [SerializeField] private Sprite _playerPushSprite;

        private List<LevelData> _levels;
        private int _levelIndex;
        private SokobanGrid _grid;
        private BoardView _board;
        private bool _transitionPending;

        private void Start()
        {
            var jsonAsset = Resources.Load<TextAsset>("Levels/levels");
            if (jsonAsset == null)
            {
                Debug.LogError("[Sokobox] Resources/Levels/levels.json not found — did the level file get imported?");
                return;
            }

            _levels = LevelParser.ParseAll(jsonAsset.text);

            _board = gameObject.AddComponent<BoardView>();
            _board.Configure(_wallSprite, _boxSprite, _goalSprite, _playerIdleSprite, _playerPushSprite);

            LoadLevel(0);
        }

        private void Update()
        {
            if (_grid == null || _transitionPending)
            {
                return;
            }

            var keyboard = Keyboard.current;
            if (keyboard == null)
            {
                return;
            }

            // Arrows + WASD, one step per key press. Buffered/held movement
            // would need a repeat timer — deliberately out of scaffold scope.
            var direction = ReadDirection(keyboard);
            if (direction.HasValue)
            {
                HandleMove(direction.Value);
            }
        }

        private static Direction? ReadDirection(Keyboard keyboard)
        {
            if (keyboard.upArrowKey.wasPressedThisFrame || keyboard.wKey.wasPressedThisFrame) return Direction.Up;
            if (keyboard.downArrowKey.wasPressedThisFrame || keyboard.sKey.wasPressedThisFrame) return Direction.Down;
            if (keyboard.leftArrowKey.wasPressedThisFrame || keyboard.aKey.wasPressedThisFrame) return Direction.Left;
            if (keyboard.rightArrowKey.wasPressedThisFrame || keyboard.dKey.wasPressedThisFrame) return Direction.Right;
            return null;
        }

        private void HandleMove(Direction direction)
        {
            var result = _grid.Move(direction);
            if (result.Type == MoveResultType.Blocked)
            {
                return;
            }

            _board.Refresh(_grid, result.Type == MoveResultType.Pushed);

            if (_grid.IsComplete())
            {
                StartCoroutine(CompleteLevelRoutine());
            }
        }

        private IEnumerator CompleteLevelRoutine()
        {
            // Same 400 ms beat as the web version before switching levels.
            // The end-of-level report screen (time + moves + next/quit)
            // replaces this in the next milestone — see GDD-dev.
            _transitionPending = true;
            yield return new WaitForSeconds(0.4f);
            _transitionPending = false;

            _levelIndex++;
            if (_levelIndex < _levels.Count)
            {
                LoadLevel(_levelIndex);
            }
            else
            {
                Debug.Log("[Sokobox] All levels complete!");
            }
        }

        private void LoadLevel(int index)
        {
            _grid = new SokobanGrid(_levels[index]);
            _board.Show(_grid);
            Debug.Log($"[Sokobox] Level {index + 1}: {_levels[index].Name}");
        }
    }
}
