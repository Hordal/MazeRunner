using UnityEngine;

public enum MazeBuffType { Speed, Time }

public class MazeBuff : MonoBehaviour
{
    [SerializeField] private MazeBuffType buffType;

    private void Update() => transform.Rotate(0f, 70f * Time.deltaTime, 0f, Space.World);

    private void OnTriggerEnter(Collider other)
    {
        FirstPersonController player = other.GetComponent<FirstPersonController>();
        if (player == null) return;

        if (buffType == MazeBuffType.Speed) player.ApplySpeedEffect(1.6f, 10f);
        else MazeGameManager.Instance.AddTime(30f);
        Destroy(gameObject);
    }

    public void SetType(MazeBuffType type) => buffType = type;
}
