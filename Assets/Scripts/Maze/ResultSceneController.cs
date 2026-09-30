using UnityEngine;
using UnityEngine.SceneManagement;

public class ResultSceneController : MonoBehaviour
{
    private void Awake()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        RenderSettings.fog = false;
    }

    private void OnGUI()
    {
        bool success = MazeRunResult.IsSuccess;
        Color accent = success ? new Color(.2f, .9f, .55f) : new Color(1f, .3f, .25f);
        GUI.color = new Color(.03f, .05f, .1f, 1f);
        GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), Texture2D.whiteTexture);
        GUI.color = Color.white;

        GUIStyle title = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, fontSize = 48, fontStyle = FontStyle.Bold, normal = { textColor = accent } };
        GUIStyle text = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, fontSize = 22, normal = { textColor = Color.white } };
        GUI.Label(new Rect(0, Screen.height / 2 - 180, Screen.width, 65), success ? "탈출 성공!" : "탈출 실패", title);
        GUI.Label(new Rect(0, Screen.height / 2 - 100, Screen.width, 35), success ? "3층 미로를 탈출했습니다." : "제한 시간이 모두 지났습니다.", text);
        if (success) GUI.Label(new Rect(0, Screen.height / 2 - 55, Screen.width, 35), "남은 시간: " + MazeRunResult.TimeRemaining.ToString("F1") + "초", text);

        float x = Screen.width / 2 - 145;
        if (GUI.Button(new Rect(x, Screen.height / 2 + 35, 290, 48), "3층 다시 도전")) SceneManager.LoadScene("Floor3_3D");
        if (GUI.Button(new Rect(x, Screen.height / 2 + 95, 290, 48), "로비로 이동")) SceneManager.LoadScene("MazeLobby");
    }
}
