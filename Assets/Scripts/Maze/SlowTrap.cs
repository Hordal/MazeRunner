using UnityEngine;

public class SlowTrap : MonoBehaviour
{
    private void OnTriggerStay(Collider other)
    {
        FirstPersonController player = other.GetComponent<FirstPersonController>();
        if (player != null) player.ApplySpeedEffect(.45f, 1.2f);
    }

    private void OnTriggerEnter(Collider other)
    {
        FirstPersonController player = other.GetComponent<FirstPersonController>();
        if (player == null) return;
        player.ApplySpeedEffect(.45f, 4f);
        MazeGameManager.Instance.ShowEvent("끈적한 함정! 이동 속도가 잠시 감소합니다.");
    }
}
