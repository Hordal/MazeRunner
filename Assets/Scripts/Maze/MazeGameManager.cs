using UnityEngine;
using UnityEngine.SceneManagement;

public enum MazeFloorType { Floor1, Floor2, Floor3 }

public class MazeGameManager : MonoBehaviour
{
    public static MazeGameManager Instance { get; private set; }
    public MazeFloorType floorType;
    public Camera mapCamera;
    public GameObject[] keyMarkers;
    public GameObject[] trapMarkers;
    public GameObject exitMarker;
    // 전체 지도 공개 중에만 보이는 평면 아이콘들이다.
    public GameObject[] mapKeyIcons;
    public GameObject[] mapItemIcons;
    public GameObject mapExitIcon;
    public GameObject mapPlayerIcon;
    public int KeysCollected { get; private set; }
    public bool HasEscaped { get; private set; }
    public bool HasFailed { get; private set; }

    private float timeRemaining;
    private float keysRevealEndsAt, exitRevealEndsAt, mapEndsAt, trapRevealEndsAt, timerPauseEndsAt;
    private string eventMessage;
    private string interactionPrompt;
    private float messageEndsAt;
    private readonly Color normalAmbient = new Color(0.04f, 0.05f, 0.09f);

    private void Awake()
    {
        Instance = this;
        timeRemaining = floorType == MazeFloorType.Floor3 ? 300f : 180f;
        if (mapCamera != null) mapCamera.enabled = false;
        SetActive(keyMarkers, false);
        if (exitMarker != null) exitMarker.SetActive(false);
        SetActive(mapKeyIcons, false);
        SetActive(mapItemIcons, false);
        if (mapExitIcon != null) mapExitIcon.SetActive(false);
        if (mapPlayerIcon != null) mapPlayerIcon.SetActive(false);
        RenderSettings.ambientLight = normalAmbient;
        RenderSettings.fog = false;
        SetActive(trapMarkers, false);
        eventMessage = floorType == MazeFloorType.Floor1 ? "열쇠 3개를 모아 출구로 가세요." : floorType == MazeFloorType.Floor2 ? "함정을 피하고 출구로 가세요. Shift로 달릴 수 있습니다." : "5분 안에 출구로 가세요. 시간 상자를 찾으세요.";
        messageEndsAt = Time.time + 3.5f;
    }

    private void Update()
    {
        bool mapVisible = Time.time < mapEndsAt;
        if (mapCamera != null) mapCamera.enabled = mapVisible;
        SetActive(keyMarkers, Time.time < keysRevealEndsAt);
        SetActive(trapMarkers, Time.time < trapRevealEndsAt);
        if (exitMarker != null) exitMarker.SetActive(Time.time < exitRevealEndsAt);
        SetActive(mapKeyIcons, mapVisible);
        SetActive(mapItemIcons, mapVisible);
        if (mapExitIcon != null) mapExitIcon.SetActive(mapVisible);
        if (mapPlayerIcon != null) mapPlayerIcon.SetActive(mapVisible);
        RenderSettings.ambientLight = mapVisible ? new Color(0.55f, 0.58f, 0.65f) : floorType == MazeFloorType.Floor3 && timeRemaining < 60f ? new Color(.2f, .04f, .04f) : normalAmbient;
        if (floorType == MazeFloorType.Floor3 && !HasEscaped && !HasFailed && timeRemaining > 0f && Time.time >= timerPauseEndsAt) timeRemaining -= Time.deltaTime;
        if (floorType == MazeFloorType.Floor3 && timeRemaining <= 0f && !HasEscaped && !HasFailed)
        {
            HasFailed = true;
            MazeRunResult.Set(false, 0f);
            SceneManager.LoadScene("ResultScene");
        }
    }

    public void CollectKey() { if (!HasEscaped) KeysCollected++; }
    public void AddTime(float seconds) { timeRemaining += seconds; }
    public void RevealFullMap(float duration = 10f)
    {
        mapEndsAt = Time.time + duration;
        ShowEvent("상자: 전체 지도 공개 " + duration.ToString("F0") + "초");
    }
    public bool CanEscape() => floorType == MazeFloorType.Floor1 ? KeysCollected >= 3 : floorType == MazeFloorType.Floor3 ? KeysCollected >= 1 : true;
    public void TryEscape()
    {
        if (!CanEscape()) { ShowEvent(floorType == MazeFloorType.Floor1 ? "출구가 잠겨 있습니다. 열쇠 3개가 필요합니다." : "출구가 잠겨 있습니다. 열쇠 1개가 필요합니다."); return; }
        HasEscaped = true;
        MazeProgress.Unlock(floorType == MazeFloorType.Floor1 ? 2 : floorType == MazeFloorType.Floor2 ? 3 : 3);
        if (MazeAudio.Instance != null) MazeAudio.Instance.PlayTone(880f, .5f);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        if (floorType == MazeFloorType.Floor3)
        {
            MazeRunResult.Set(true, Mathf.Max(0f, timeRemaining));
            SceneManager.LoadScene("ResultScene");
        }
    }
    public void UseFloor1ChestEffect(FirstPersonController player)
    {
        switch (Random.Range(0, 5))
        {
            case 0: keysRevealEndsAt = Time.time + 60f; ShowEvent("상자: 열쇠 위치 공개 1분"); break;
            case 1: exitRevealEndsAt = Time.time + 60f; ShowEvent("상자: 출구 위치 공개 1분"); break;
            case 2: RevealFullMap(); break;
            case 3: player.ApplySpeedEffect(1.6f, 10f); ShowEvent("상자: 10초 동안 이동 속도 증가"); break;
            default: player.ApplySpeedEffect(.45f, 8f); ShowEvent("상자: 8초 동안 이동 속도 감소"); break;
        }
    }
    public void UseFloor2Special(FirstPersonController player)
    {
        switch (Random.Range(0, 3))
        {
            case 0: player.RestoreStamina(60f); ShowEvent("상자: 스태미나 60 회복"); break;
            case 1: player.ApplyInfiniteStamina(10f); ShowEvent("상자: 10초 동안 스태미나 무한"); break;
            default: trapRevealEndsAt = Time.time + 15f; ShowEvent("상자: 함정 위치 공개 15초"); break;
        }
    }
    public void UseFloor3Special()
    {
        if (Random.Range(0, 2) == 0) { AddTime(30f); ShowEvent("특수 상자: 제한 시간 +30초"); }
        else { AddTime(-20f); ShowEvent("특수 상자: 제한 시간 -20초"); }
    }
    public void ShowEvent(string message) { eventMessage = message; messageEndsAt = Time.time + 4f; }
    public void SetInteractionPrompt(string message) { interactionPrompt = message; }
    // 이전 출구 잠금 함정과의 호환용. 현재 2층 출구는 항상 사용할 수 있다.
    public void AllowFloor2Exit() { ShowEvent("함정 통과 성공!"); }
    private static void SetActive(GameObject[] objects, bool active) { if (objects == null) return; foreach (GameObject item in objects) if (item != null) item.SetActive(active); }

    private void OnGUI()
    {
        GUIStyle label = new GUIStyle(GUI.skin.label) { fontSize = 22, fontStyle = FontStyle.Bold, normal = { textColor = Color.white } };
        if (floorType == MazeFloorType.Floor1 || floorType == MazeFloorType.Floor3) GUI.Label(new Rect(24, 20, 500, 35), "열쇠  " + KeysCollected + " / " + (floorType == MazeFloorType.Floor1 ? 3 : 1), label);
        FirstPersonController player = FindFirstObjectByType<FirstPersonController>();
        if (player != null) GUI.Label(new Rect(24, floorType == MazeFloorType.Floor3 ? 80 : floorType == MazeFloorType.Floor1 ? 50 : 20, 500, 35), "스태미나  " + player.Stamina.ToString("F0") + " / 100", label);
        if (floorType == MazeFloorType.Floor3) GUI.Label(new Rect(24, 50, 500, 35), "남은 시간  " + Mathf.Max(0f, timeRemaining).ToString("F1") + "초", label);
        GUIStyle hint = new GUIStyle(label) { fontSize = 16 };
        GUI.Label(new Rect(24, Screen.height - 44, 700, 30), "WASD 이동  |  마우스 시점  |  ESC 환경설정", hint);
        if (!string.IsNullOrEmpty(interactionPrompt)) GUI.Label(new Rect(0, Screen.height - 145, Screen.width, 35), interactionPrompt, new GUIStyle(label) { alignment = TextAnchor.MiddleCenter });
        if (Time.time < messageEndsAt) { GUIStyle message = new GUIStyle(label) { alignment = TextAnchor.MiddleCenter, fontSize = 26 }; GUI.Label(new Rect(0, Screen.height - 100, Screen.width, 40), eventMessage, message); }
        if (Time.time < mapEndsAt)
        {
            GUIStyle mapLegend = new GUIStyle(label) { fontSize = 16, alignment = TextAnchor.UpperLeft, normal = { textColor = new Color(.08f, .12f, .18f) } };
            float legendY = Screen.height * .56f;
            GUI.Box(new Rect(Screen.width - 330, legendY, 300, 98), "지도 아이콘");
            GUI.Label(new Rect(Screen.width - 312, legendY + 28, 280, 70), "하늘색: 나의 위치 / 방향\n노랑: 열쇠   주황: 상자\n초록: 출구", mapLegend);
        }
        if (HasEscaped)
        {
            GUIStyle clear = new GUIStyle(label) { alignment = TextAnchor.MiddleCenter, fontSize = 42 };
            GUI.Label(new Rect(0, Screen.height / 2 - 80, Screen.width, 90), "탈출 성공!", clear);
            string next = floorType == MazeFloorType.Floor1 ? "2층으로 이동" : floorType == MazeFloorType.Floor2 ? "3층으로 이동" : "로비로 이동";
            if (GUI.Button(new Rect(Screen.width / 2 - 110, Screen.height / 2 + 5, 220, 46), next)) SceneManager.LoadScene(floorType == MazeFloorType.Floor1 ? "Floor2_3D" : floorType == MazeFloorType.Floor2 ? "Floor3_3D" : "MazeLobby");
        }
        if (HasFailed)
        {
            GUIStyle clear = new GUIStyle(label) { alignment = TextAnchor.MiddleCenter, fontSize = 40 };
            GUI.Label(new Rect(0, Screen.height / 2 - 85, Screen.width, 70), "시간이 초과되었습니다", clear);
            if (GUI.Button(new Rect(Screen.width / 2 - 110, Screen.height / 2, 220, 42), "다시 도전")) SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            if (GUI.Button(new Rect(Screen.width / 2 - 110, Screen.height / 2 + 52, 220, 42), "로비로 돌아가기")) SceneManager.LoadScene("MazeLobby");
        }
    }
}
