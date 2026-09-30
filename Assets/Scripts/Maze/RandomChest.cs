using UnityEngine;
using UnityEngine.InputSystem;

public class RandomChest : MonoBehaviour
{
    [SerializeField] private bool guaranteedMapReveal;
    private bool opened;
    private FirstPersonController nearbyPlayer;
    private void OnTriggerEnter(Collider other)
    {
        if (opened) return;
        FirstPersonController player = other.GetComponent<FirstPersonController>();
        if (player == null) return;
        nearbyPlayer = player;
        MazeGameManager.Instance.SetInteractionPrompt("E 키를 눌러 상자 열기");
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.GetComponent<FirstPersonController>() == null) return;
        nearbyPlayer = null;
        MazeGameManager.Instance.SetInteractionPrompt("");
    }
    private void Update()
    {
        if (opened || nearbyPlayer == null || Keyboard.current == null || !Keyboard.current.eKey.wasPressedThisFrame) return;
        opened = true;
        MazeGameManager.Instance.SetInteractionPrompt("");
        if (MazeAudio.Instance != null) MazeAudio.Instance.PlayTone(740f, .22f);
        if (guaranteedMapReveal) MazeGameManager.Instance.RevealFullMap();
        else if (MazeGameManager.Instance.floorType == MazeFloorType.Floor1) MazeGameManager.Instance.UseFloor1ChestEffect(nearbyPlayer);
        else if (MazeGameManager.Instance.floorType == MazeFloorType.Floor2) MazeGameManager.Instance.UseFloor2Special(nearbyPlayer);
        else MazeGameManager.Instance.UseFloor3Special();
        Destroy(gameObject);
    }

    public void SetGuaranteedMapReveal() => guaranteedMapReveal = true;
}
