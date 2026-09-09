using System.Threading;
using Cysharp.Threading.Tasks;
using Driver.Data.Scriptable;
using Driver.Gameplay.Characters;
using Driver.Gameplay.Level;
using UnityEngine;

namespace Driver.Gameplay.Flow
{
    public class LevelOutro
    {
        private readonly Car car;
        private readonly CameraFollow cameraFollow;
        private readonly Camera levelCamera;
        private readonly LevelOutroConfig config;

        public LevelOutro(Car car, CameraFollow cameraFollow, Camera levelCamera, LevelOutroConfig config)
        {
            this.car = car;
            this.cameraFollow = cameraFollow;
            this.levelCamera = levelCamera;
            this.config = config;
        }

        public async UniTask PlayAsync(CancellationToken token)
        {
            cameraFollow.Freeze();
            car.StartMovingAway(config.AlignDuration, config.MoveAwaySpeedMultiplier);

            float deadline = Time.time + config.MaxDuration;

            await UniTask.WaitUntil(() => HasLeftView() || Time.time >= deadline, cancellationToken: token);
        }

        private bool HasLeftView()
        {
            Vector3 view = levelCamera.WorldToViewportPoint(car.transform.position);

            return view.z <= 0f || view.y > 1.15f;
        }
    }
}