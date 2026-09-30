using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class MazeSettingsUI : MonoBehaviour
{
    private bool open;
    private float volume = 1f;
    private float sensitivity = 1f;

    private void Update()
    {
        if (Keyboard.current != null && (Keyboard.current.escapeKey.wasPressedThisFrame || Keyboard.current.tabKey.wasPressedThisFrame))
        {
            open = !open;
            UpdateCursorState();
        }
    }

    private void OnGUI()
    {
        if (GUI.Button(new Rect(Screen.width - 130, 20, 110, 34), "환경설정"))
        {
            open = !open;
            UpdateCursorState();
        }
        if (!open) return;
        GUI.Box(new Rect(Screen.width / 2 - 190, Screen.height / 2 - 165, 380, 330), "환경설정");
        GUI.Label(new Rect(Screen.width / 2 - 155, Screen.height / 2 - 110, 150, 25), "전체 음량");
        volume = GUI.HorizontalSlider(new Rect(Screen.width / 2 - 20, Screen.height / 2 - 105, 150, 20), volume, 0f, 1f);
        AudioListener.volume = volume;
        GUI.Label(new Rect(Screen.width / 2 - 155, Screen.height / 2 - 65, 180, 25), "마우스 감도: " + sensitivity.ToString("F1"));
        sensitivity = GUI.HorizontalSlider(new Rect(Screen.width / 2 - 20, Screen.height / 2 - 60, 150, 20), sensitivity, .25f, 4f);
        FirstPersonController.MouseSensitivityMultiplier = sensitivity;
        if (GUI.Button(new Rect(Screen.width / 2 - 150, Screen.height / 2 - 10, 300, 35), Screen.fullScreen ? "창 모드로 전환" : "전체 화면으로 전환")) Screen.fullScreen = !Screen.fullScreen;
        if (GUI.Button(new Rect(Screen.width / 2 - 150, Screen.height / 2 + 35, 300, 35), "로비로 돌아가기")) SceneManager.LoadScene("MazeLobby");
        if (GUI.Button(new Rect(Screen.width / 2 - 150, Screen.height / 2 + 80, 300, 35), "닫기")) { open = false; UpdateCursorState(); }
    }

    private void UpdateCursorState()
    {
        bool keepCursorVisible = MazeGameManager.Instance != null && (MazeGameManager.Instance.HasEscaped || MazeGameManager.Instance.HasFailed);
        Cursor.lockState = open || keepCursorVisible ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = open || keepCursorVisible;
    }
}
