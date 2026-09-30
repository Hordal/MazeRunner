using UnityEngine;

public class MazeKey : MonoBehaviour
{
    private void Update() => transform.Rotate(0f, 100f * Time.deltaTime, 0f, Space.World);

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<FirstPersonController>() == null) return;
        MazeGameManager.Instance.CollectKey();
        Destroy(gameObject);
    }
}
