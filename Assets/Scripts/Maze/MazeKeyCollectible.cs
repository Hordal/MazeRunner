using UnityEngine;

public class MazeKeyCollectible : MonoBehaviour
{
    private void Update() => transform.Rotate(0f, 100f * Time.deltaTime, 0f, Space.World);
    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<FirstPersonController>() == null) return;
        MazeGameManager.Instance.CollectKey();
        MazeGameManager.Instance.ShowEvent("열쇠를 획득했습니다.");
        if (MazeAudio.Instance != null) MazeAudio.Instance.PlayTone(990f, .18f);
        Destroy(gameObject);
    }
}
