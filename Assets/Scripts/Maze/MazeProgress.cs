using UnityEngine;

public static class MazeProgress
{
    private const string UnlockedFloorKey = "MazeUnlockedFloor";
    public static int UnlockedFloor => Mathf.Clamp(PlayerPrefs.GetInt(UnlockedFloorKey, 1), 1, 3);
    public static void Unlock(int floor) { if (floor > UnlockedFloor) { PlayerPrefs.SetInt(UnlockedFloorKey, floor); PlayerPrefs.Save(); } }
}
