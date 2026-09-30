using UnityEngine;

public class MazeCheckpoint : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        FirstPersonController player = other.GetComponent<FirstPersonController>();
        if (player == null) return;
        player.SetCheckpoint(transform.position + Vector3.forward * .8f + Vector3.up * 1.05f);
        MazeGameManager.Instance.ShowEvent("체크포인트가 저장되었습니다.");
    }
}
