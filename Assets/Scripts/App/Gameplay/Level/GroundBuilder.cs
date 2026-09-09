using System.Collections.Generic;
using Driver.Data.Scriptable;
using UnityEngine;

namespace Driver.Gameplay.Level
{
    public class GroundBuilder : MonoBehaviour
    {
        private List<GameObject> tiles;

        private void Awake()
        {
            tiles = new List<GameObject>();
        }

        public void Build(LevelConfig levelConfig)
        {
            Clear();

            if (levelConfig == null || levelConfig.GroundTilePrefab == null)
                return;

            float tileLength = levelConfig.GroundTileLength;
            float start = -levelConfig.GroundPaddingBefore;
            float end = levelConfig.Length + levelConfig.GroundPaddingAfter;

            int index = 0;
            for (float z = start; z < end; z += tileLength, index++)
            {
                GameObject tile = Instantiate(levelConfig.GroundTilePrefab, transform);
                tile.transform.localRotation = Quaternion.identity;
                tile.transform.localPosition = new Vector3(0f, 0f, z + tileLength * 0.5f);
                tile.name = $"GroundTile_{index:00}";

                tiles.Add(tile);
            }
        }

        public void Clear()
        {
            foreach (GameObject tile in tiles)
            {
                if (tile != null)
                    Destroy(tile);
            }

            tiles.Clear();
        }
    }
}