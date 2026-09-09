using System;

namespace Driver.Managers.Interfaces
{
    public interface IDataManager
    {
        bool IsDataLoaded { get; }
        DataManager.CachedData UserData { get; }

        void LoadUserData();
        void SaveUserData();

        void RegisterOnDataLoadedEvent(Action<DataManager.CachedData> listener);
        void UnregisterOnDataLoadedEvent(Action<DataManager.CachedData> listener);
    }
}