using UnityEngine;

public class MazeExit : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<FirstPersonController>() != null)
            MazeGameManager.Instance.TryEscape();
    }
}
