using Cysharp.Threading.Tasks;
using Driver.Base.ObjectPool;
using Driver.Controllers.Interfaces;
using Driver.Data.Scriptable;
using Driver.Gameplay.Characters;
using Driver.Gameplay.Characters.Enemies;
using Driver.Gameplay.Characters.Enemies.Components;
using Driver.Gameplay.Flow;
using Driver.Gameplay.Input;
using Driver.Gameplay.Vfx;
using System;
using System.Linq;
using System.Threading;
using UnityEngine;

namespace Driver.Gameplay.Level
{
    public class Level : MonoBehaviour
    {
        public event Action<float, CharacterType, Vector3> OnCharacterDamagedEvent;
        public event Action<bool> OnRunningChanged;

        public Car Car => car;
        public Camera LevelCamera => levelCamera;
        public bool IsRunning => isRunning;
        public TimeSpan RunScore => TimeSpan.FromSeconds(runTime);

        [SerializeField] private GroundBuilder groundBuilder;
        [SerializeField] private Transform carParent;
        [SerializeField] private Transform enemiesParent;
        [SerializeField] private Transform vfxParent;
        [SerializeField] private Transform bulletsParent;

        private LevelConfig levelConfig;
        private CarConfig carConfig;
        private ITimerController timerController;
        private IGameplayInput input;
        private EnemyController enemyController;

        private Camera levelCamera;
        private CameraFollow cameraFollow;

        private Car car;
        private ObjectPoolsController<Enemy, Enemy.ActivationData> enemyPools;
        private VfxController vfxController;
        private LevelOutro levelOutro;

        private bool isRunning;
        private float runTime;

        public void Init(LevelConfig levelConfig, LevelOutroConfig outroConfig, CarConfig carConfig, GameObject carPrefab,
            VfxConfig vfxConfig, EnemiesConfig enemiesConfig, ITimerController timerController, IGameplayInput input,
            Camera levelCamera, CameraFollow cameraFollow)
        {
            this.levelConfig = levelConfig;
            this.carConfig = carConfig;
            this.timerController = timerController;
            this.input = input;
            this.levelCamera = levelCamera;
            this.cameraFollow = cameraFollow;

            vfxController = new VfxController(vfxConfig, vfxParent);

            groundBuilder.Build(levelConfig);

            car = Instantiate(carPrefab, carParent).GetComponent<Car>();

            cameraFollow.SetTarget(car.transform);

            levelOutro = new LevelOutro(car, cameraFollow, levelCamera, outroConfig);

            enemyController = new EnemyController(enemiesConfig, levelConfig, levelCamera, enemiesParent);

            var enemyPoolWarmupInfos = enemyController.GetWarmupInfos();
            enemyPools = new ObjectPoolsController<Enemy, Enemy.ActivationData>(enemyPoolWarmupInfos.Select(it =>
                new IdentifiedObjectPoolInfo(
                    it.Config.GetId(),
                    new MonoBehaviourPoolInitData(it.Config.Prefab, it.InitialPoolSize, enemiesParent))));

            enemyController.OnEnemySpawnEvent += OnEnemySpawnEventHandler;

            ResetToStart();
        }

        public void StartRun()
        {
            SetRunning(true);

            car.StartDriving();
        }

        public void StopRun()
        {
            SetRunning(false);

            car.StopDriving();

            StopAllEnemies();
        }

        public async UniTask PlayOutroAsync(CancellationToken token)
        {
            SetRunning(false);

            car.SetTurretFiring(false);
            StopAllEnemies();

            await levelOutro.PlayAsync(token);

            car.StopDriving();
        }

        public void ResetToStart()
        {
            SetRunning(false);
            runTime = 0f;

            StopAllEnemies();
            enemyController.Reset();

            InitCar();

            cameraFollow.SnapToTarget();
        }

        private void SetRunning(bool value)
        {
            if (isRunning == value)
                return;

            isRunning = value;

            OnRunningChanged?.Invoke(value);
        }

        public bool IsFinishReached()
        {
            return car != null && car.DistanceTravelled >= levelConfig.Length;
        }

        public float GetProgressByDistance()
        {
            if (car == null || levelConfig.Length <= 0f)
                return 0f;

            return Mathf.Clamp01(car.DistanceTravelled / levelConfig.Length);
        }

        private void Update()
        {
            if (!isRunning)
                return;

            runTime += Time.deltaTime;

            enemyController.Update(Time.deltaTime);

            if (input.FireHeld)
            {
                car.AimTurret(input.PointerScreenPosition);
            }

            car.SetTurretFiring(input.FireHeld);
        }

        private void InitCar()
        {
            car.Init(carConfig, timerController, levelCamera, bulletsParent);

            car.OnDamagedEvent -= OnCharacterDamagedEventHandler;
            car.OnDamagedEvent += OnCharacterDamagedEventHandler;
        }

        private void StopAllEnemies()
        {
            if (enemyPools == null)
                return;

            foreach (ObjectPoolHandle<Enemy> handle in enemyPools.GetAllActiveItems())
            {
                if (handle.TryResolve(out Enemy enemy))
                {
                    enemy.Deactivate();
                }
            }
        }

        private void OnCharacterDamagedEventHandler(float damage, CharacterType characterType, Vector3 worldPosition)
        {
            OnCharacterDamagedEvent?.Invoke(damage, characterType, worldPosition);

            if (characterType == CharacterType.Enemy)
                vfxController.Play(VfxType.BulletImpact, worldPosition + Vector3.up, Quaternion.identity);
        }

        private void OnEnemySpawnEventHandler(EnemyConfig config, Vector3 trackPosition)
        {
            if (!enemyPools.TryGetMonoBehaviourObjectPoolByIdentifier(config.GetId(),
                    out IdentifiedMonoBehaviourObjectPool<Enemy, Enemy.ActivationData> pool))
                return;

            Vector3 worldPosition = enemiesParent.TransformPoint(trackPosition);

            ObjectPoolHandle<Enemy> handle = pool.ReleseObject(
                new Enemy.ActivationData(config, timerController, car.transform, car.BodyCollider, worldPosition));

            if (handle.TryResolve(out Enemy enemy))
            {
                enemy.OnDamagedEvent += OnCharacterDamagedEventHandler;
                enemy.OnHealthOutEvent += () => OnEnemyDiedEventHandler(enemy);

                enemyController.Register(enemy, config.OffscreenLifetime);
            }
        }

        private void OnEnemyDiedEventHandler(Enemy enemy)
        {
            vfxController.Play(VfxType.EnemyDeath, enemy.transform.position + Vector3.up, Quaternion.identity);
        }
    }
}