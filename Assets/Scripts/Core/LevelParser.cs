using System;
using System.Collections.Generic;
using UnityEngine;

namespace Sokobox.Core
{
    /// <summary>
    /// Turns the JSON level files (same format as the web prototype:
    /// a "levels" array of { id, name, map }) into LevelData objects.
    /// JsonUtility is used for deserialization — it lives in UnityEngine,
    /// but it is deterministic enough that the parser stays fully testable
    /// in EditMode tests.
    /// </summary>
    public static class LevelParser
    {
        [Serializable]
        private class LevelFile
        {
            public List<LevelDto> levels;
        }

        [Serializable]
        private class LevelDto
        {
            public int id;
            public string name;
            public List<string> map;
        }

        public static List<LevelData> ParseAll(string json)
        {
            var file = JsonUtility.FromJson<LevelFile>(json);
            if (file?.levels == null)
            {
                throw new ArgumentException("Level file has no 'levels' array.");
            }

            var levels = new List<LevelData>(file.levels.Count);
            foreach (var dto in file.levels)
            {
                levels.Add(Parse(dto.name, dto.map));
            }
            return levels;
        }

        /// <summary>
        /// Parses one ASCII map. Legend (identical to the web version):
        /// '#' wall, '@' player start, '$' box, '.' target, ' ' floor.
        /// </summary>
        public static LevelData Parse(string name, List<string> map)
        {
            if (map == null || map.Count == 0)
            {
                throw new ArgumentException($"Level '{name}' has an empty map.");
            }

            var walls = new HashSet<GridPosition>();
            var boxes = new List<GridPosition>();
            var targets = new HashSet<GridPosition>();
            var playerStart = new GridPosition(-1, -1);

            for (var row = 0; row < map.Count; row++)
            {
                var line = map[row];
                for (var col = 0; col < line.Length; col++)
                {
                    var cell = new GridPosition(col, row);
                    switch (line[col])
                    {
                        case '#': walls.Add(cell); break;
                        case '$': boxes.Add(cell); break;
                        case '.': targets.Add(cell); break;
                        case '@': playerStart = cell; break;
                    }
                }
            }

            if (playerStart.Col < 0)
            {
                throw new ArgumentException($"Level '{name}' has no player start (@).");
            }

            return new LevelData(name, map[0].Length, map.Count, playerStart, walls, boxes, targets);
        }
    }
}
