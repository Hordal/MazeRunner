public static class MazeRunResult
{
    public static bool IsSuccess { get; private set; }
    public static float TimeRemaining { get; private set; }

    public static void Set(bool success, float timeRemaining)
    {
        IsSuccess = success;
        TimeRemaining = timeRemaining;
    }
}
