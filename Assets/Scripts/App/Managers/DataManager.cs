using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Driver.Controllers;
using Driver.Controllers.Interfaces;
using Driver.Data;
using Driver.Extensions;
using Driver.Managers.Interfaces;
using UnityEngine;
using Zenject;

namespace Driver.Managers
{
    public class DataManager : MonoBehaviour, IDataManager
    {
        private const float AutoSaveDelay = 10f;

        private event Action<CachedData> OnDataLoadedEvent;

        public bool IsDataLoaded => UserData?.IsDataLoaded ?? false;
        public CachedData UserData { get; private set; }

        [Inject] private ITimerController timerManager;

        private TimerController.ITimer autoSaveTimer;
        private Task autoSaveTask;

        [Inject]
        private void Construct()
        {
            UserData = new();
        }

        private void OnApplicationQuit()
        {
            StopAutoSave();
            SaveUserData();
        }

        public void LoadUserData()
        {
            if (!IsDataLoaded)
            {
                LoadUserDataAsync((data) =>
                {
                    OnDataLoadedEvent?.Invoke(data);
                    StartAutoSave();
                });
            }
        }

        public void SaveUserData()
        {
            if (IsDataLoaded)
            {
                UserData.SaveAllData();
            }
        }

        public void RegisterOnDataLoadedEvent(Action<CachedData> listener)
        {
            OnDataLoadedEvent += listener;

            if (IsDataLoaded)
            {
                listener?.Invoke(UserData);
            }
        }

        public void UnregisterOnDataLoadedEvent(Action<CachedData> listener)
        {
            OnDataLoadedEvent -= listener;
        }

        private async void LoadUserDataAsync(Action<CachedData> callback)
        {
            Debug.Log("Start loading user data.");
            await UserData.LoadAllDataAsync();
            Debug.Log("User data loaded.");
            callback?.Invoke(UserData);
        }

        private void StartAutoSave()
        {
            autoSaveTimer = timerManager.CreateTimer(AutoSaveDelay, HandleAutoSave);
        }

        private void StopAutoSave()
        {
            timerManager.RemoveTimer(autoSaveTimer);

            if (autoSaveTask != null)
            {
                autoSaveTask.Dispose();
                autoSaveTask = null;
            }
        }

        private void HandleAutoSave()
        {
            if (IsDataLoaded)
            {
                autoSaveTask = UserData.SaveAllDataAsync();
            }

            StartAutoSave();
        }

        public class CachedData
        {
            public static readonly string BaseDataPath = $"{Application.persistentDataPath}/CacheData/";

            private readonly Dictionary<GameDataType, string> GameDataPaths;

            public bool IsDataLoaded { get; private set; }
            public UserProgression UserProgression { get; private set; }

            public CachedData()
            {
                GameDataPaths = new()
                {
                    { GameDataType.UserProgression, $"{BaseDataPath}{UserProgression.FileName}" }
                };

                if (!Directory.Exists(BaseDataPath))
                {
                    Directory.CreateDirectory(BaseDataPath);
                }
            }

            public async Task LoadAllDataAsync()
            {
                List<Task> tasks = new()
                {
                    LoadDataAsync(GameDataType.UserProgression)
                };

                await Task.WhenAll(tasks);

                IsDataLoaded = true;
            }

            public void SaveAllData()
            {
                foreach (GameDataType key in GameDataPaths.Keys)
                {
                    SaveData(key);
                }
            }

            public void SaveData(GameDataType gameDataType)
            {
                string data = string.Empty;
                string dataPath = GameDataPaths[gameDataType];

                switch (gameDataType)
                {
                    case GameDataType.UserProgression:
                        data = SecureCacheSerializer.Serialize(UserProgression);
                        break;
                }

                if (data.Length > 0)
                {
                    if (!File.Exists(dataPath))
                    {
                        File.Create(dataPath).Close();
                    }

                    File.WriteAllText(dataPath, data);
                }
            }

            public async Task SaveAllDataAsync()
            {
                List<Task> tasks = new()
                {
                    SaveDataAsync(GameDataType.UserProgression)
                };

                await Task.WhenAll(tasks);
            }

            public async Task SaveDataAsync(GameDataType gameDataType)
            {
                string data = string.Empty;
                string dataPath = GameDataPaths[gameDataType];

                switch (gameDataType)
                {
                    case GameDataType.UserProgression:
                        data = await SecureCacheSerializer.SerializeAsync(UserProgression);
                        break;
                }

                if (data.Length > 0)
                {
                    if (!File.Exists(dataPath))
                    {
                        File.Create(dataPath).Close();
                    }

                    await File.WriteAllTextAsync(dataPath, data);
                }
            }

            private async Task LoadDataAsync(GameDataType gameDataType)
            {
                string dataPath = GameDataPaths[gameDataType];

                switch (gameDataType)
                {
                    case GameDataType.UserProgression:
                        {
                            var result = await SecureCacheSerializer.TryDeserializeFromPathAsync<UserProgression>(dataPath);

                            if (result.success)
                            {
                                UserProgression = result.data;
                            }
                            else
                            {
                                UserProgression = UserProgression.Create();
                                SaveData(GameDataType.UserProgression);
                            }
                        }
                        break;
                }
            }

            public enum GameDataType
            {
                UserProgression
            }
        }
    }
}