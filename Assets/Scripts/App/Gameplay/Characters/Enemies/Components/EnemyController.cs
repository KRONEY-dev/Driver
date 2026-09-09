using System;
using System.Collections.Generic;
using Driver.Data.Scriptable;
using Driver.Extensions;
using UnityEngine;

namespace Driver.Gameplay.Characters.Enemies.Components
{
    public class EnemyController
    {
        public event Action<EnemyConfig, Vector3> OnEnemySpawnEvent;

        private readonly EnemiesConfig enemiesConfig;
        private readonly LevelConfig levelConfig;
        private readonly Camera trackingCamera;
        private readonly Transform track;

        private readonly List<SpawnRequest> pendingSpawns;
        private readonly Dictionary<Enemy, TrackedEnemy> trackedEnemies;
        private readonly List<Enemy> finishedEnemies;

        private int nextSpawnIndex;

        public EnemyController(EnemiesConfig enemiesConfig, LevelConfig levelConfig, Camera trackingCamera, Transform track)
        {
            this.enemiesConfig = enemiesConfig;
            this.levelConfig = levelConfig;
            this.trackingCamera = trackingCamera;
            this.track = track;

            pendingSpawns = new List<SpawnRequest>();
            trackedEnemies = new Dictionary<Enemy, TrackedEnemy>();
            finishedEnemies = new List<Enemy>();

            BuildSpawnRequests();
        }

        public IEnumerable<EnemyWarmupInfo> GetWarmupInfos()
        {
            var maxCounts = new Dictionary<EnemyType, int>();

            foreach (EnemySpawnEntry entry in levelConfig.Spawns)
            {
                maxCounts.TryGetValue(entry.EnemyType, out int current);
                maxCounts[entry.EnemyType] = current + entry.Count;
            }

            foreach (var pair in maxCounts)
            {
                if (enemiesConfig.TryGetByType(pair.Key, out EnemyConfig config))
                {
                    yield return new EnemyWarmupInfo(config, Mathf.Max(pair.Value, config.InitialPoolSize));
                }
            }
        }

        public void Register(Enemy enemy, float offscreenLifetime)
        {
            trackedEnemies[enemy] = new TrackedEnemy(offscreenLifetime);
        }

        public void Reset()
        {
            nextSpawnIndex = 0;

            pendingSpawns.Clear();
            trackedEnemies.Clear();
            finishedEnemies.Clear();

            BuildSpawnRequests();
        }

        public void Update(float deltaTime)
        {
            SpawnDueEnemies();
            ReapOffscreenEnemies(deltaTime);
        }

        private void SpawnDueEnemies()
        {
            float horizonDistance = GetSpawnHorizonDistance();

            while (nextSpawnIndex < pendingSpawns.Count && pendingSpawns[nextSpawnIndex].Distance <= horizonDistance)
            {
                SpawnRequest request = pendingSpawns[nextSpawnIndex];
                nextSpawnIndex++;

                if (enemiesConfig.TryGetByType(request.Type, out EnemyConfig config))
                {
                    OnEnemySpawnEvent?.Invoke(config, request.Position);
                }
            }
        }

        private float GetSpawnHorizonDistance()
        {
            Ray ray = trackingCamera.ViewportPointToRay(new Vector3(0.5f, 1f, 0f));
            Plane ground = new Plane(Vector3.up, track.position);

            if (!ground.Raycast(ray, out float enter))
                return float.MaxValue;

            return track.InverseTransformPoint(ray.GetPoint(enter)).z;
        }

        private void ReapOffscreenEnemies(float deltaTime)
        {
            finishedEnemies.Clear();

            foreach (var pair in trackedEnemies)
            {
                Enemy enemy = pair.Key;

                if (enemy == null || !enemy.isActiveAndEnabled || !enemy.IsAlive)
                {
                    finishedEnemies.Add(enemy);
                    continue;
                }

                if (trackingCamera.IsInView(enemy.transform.position + Vector3.up))
                {
                    pair.Value.OffscreenTime = 0f;
                    continue;
                }

                pair.Value.OffscreenTime += deltaTime;

                if (pair.Value.OffscreenTime < pair.Value.OffscreenLifetime)
                {
                    continue;
                }

                finishedEnemies.Add(enemy);
                enemy.RequestDeactivation();
            }

            foreach (Enemy enemy in finishedEnemies)
            {
                trackedEnemies.Remove(enemy);
            }
        }

        private void BuildSpawnRequests()
        {
            foreach (EnemySpawnEntry entry in levelConfig.Spawns)
            {
                for (int i = 0; i < entry.Count; i++)
                {
                    float distance = UnityEngine.Random.Range(entry.FromDistance, entry.ToDistance);
                    float lateral = UnityEngine.Random.Range(-entry.LateralRange, entry.LateralRange);

                    pendingSpawns.Add(new SpawnRequest(entry.EnemyType, distance, new Vector3(lateral, 0f, distance)));
                }
            }

            pendingSpawns.Sort((a, b) => a.Distance.CompareTo(b.Distance));
        }

        private readonly struct SpawnRequest
        {
            public readonly EnemyType Type;
            public readonly float Distance;
            public readonly Vector3 Position;

            public SpawnRequest(EnemyType type, float distance, Vector3 position)
            {
                Type = type;
                Distance = distance;
                Position = position;
            }
        }

        public readonly struct EnemyWarmupInfo
        {
            public readonly EnemyConfig Config;
            public readonly int InitialPoolSize;

            public EnemyWarmupInfo(EnemyConfig config, int initialPoolSize)
            {
                Config = config;
                InitialPoolSize = initialPoolSize;
            }
        }

        private class TrackedEnemy
        {
            public readonly float OffscreenLifetime;
            public float OffscreenTime;

            public TrackedEnemy(float offscreenLifetime)
            {
                OffscreenLifetime = offscreenLifetime;
            }
        }
    }
}