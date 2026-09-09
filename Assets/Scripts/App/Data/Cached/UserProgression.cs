namespace Driver.Data
{
    public class UserProgression
    {
        public const string FileName = "userProgression.dat";

        public int CurrentLevelIndex;

        public static UserProgression Create()
        {
            return new()
            {
                CurrentLevelIndex = 0
            };
        }

        public void OnLevelCompleted()
        {
            CurrentLevelIndex++;
        }
    }
}