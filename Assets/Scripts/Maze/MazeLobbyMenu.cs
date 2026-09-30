using UnityEngine;
using UnityEngine.SceneManagement;

public class MazeLobbyMenu : MonoBehaviour
{
    private bool settingsOpen;
    private float volume = 1f;
    private float sensitivity = 1f;

    private void Awake()
    {
        RenderSettings.fog = false;
        RenderSettings.ambientLight = new Color(.04f, .05f, .09f);
    }

    private void OnGUI()
    {
        GUIStyle title = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, fontSize = 42, fontStyle = FontStyle.Bold, normal = { textColor = Color.white } };
        GUI.Label(new Rect(0, Screen.height / 2 - 180, Screen.width, 60), "미로 탈출", title);
        float x = Screen.width / 2 - 140; float y = Screen.height / 2 - 80;
        int floor = MazeProgress.UnlockedFloor;
        GUI.Label(new Rect(0, Screen.height / 2 - 115, Screen.width, 25), "현재 진행: " + floor + "층", new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, normal = { textColor = Color.white } });
        if (GUI.Button(new Rect(x, y, 280, 50), "시작하기")) SceneManager.LoadScene(floor == 1 ? "Floor1_3D" : floor == 2 ? "Floor2_3D" : "Floor3_3D");
        if (GUI.Button(new Rect(x, y + 65, 280, 50), "환경설정")) settingsOpen = !settingsOpen;
        if (!settingsOpen) return;

        GUI.Box(new Rect(Screen.width / 2 - 190, Screen.height / 2 + 10, 380, 245), "환경설정");
        GUI.Label(new Rect(Screen.width / 2 - 155, Screen.height / 2 + 55, 150, 25), "전체 음량");
        volume = GUI.HorizontalSlider(new Rect(Screen.width / 2 - 20, Screen.height / 2 + 60, 150, 20), volume, 0f, 1f);
        AudioListener.volume = volume;
        GUI.Label(new Rect(Screen.width / 2 - 155, Screen.height / 2 + 100, 180, 25), "마우스 감도: " + sensitivity.ToString("F1"));
        sensitivity = GUI.HorizontalSlider(new Rect(Screen.width / 2 - 20, Screen.height / 2 + 105, 150, 20), sensitivity, .25f, 4f);
        FirstPersonController.MouseSensitivityMultiplier = sensitivity;
        if (GUI.Button(new Rect(Screen.width / 2 - 150, Screen.height / 2 + 145, 300, 32), Screen.fullScreen ? "창 모드" : "전체 화면")) Screen.fullScreen = !Screen.fullScreen;
        if (GUI.Button(new Rect(Screen.width / 2 - 150, Screen.height / 2 + 185, 300, 32), "닫기")) settingsOpen = false;
    }
}
