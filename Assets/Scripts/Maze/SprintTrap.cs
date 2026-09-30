using UnityEngine;

public class SprintTrap : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        FirstPersonController player = other.GetComponent<FirstPersonController>();
        if (player == null) return;
        if (player.IsSprinting) { MazeGameManager.Instance.AllowFloor2Exit(); MazeGameManager.Instance.ShowEvent("함정 통과 성공! 출구로 이동하세요."); }
        else { MazeGameManager.Instance.ShowEvent("달리지 못해 체크포인트로 돌아갑니다."); player.ReturnToCheckpoint(); }
    }
}
