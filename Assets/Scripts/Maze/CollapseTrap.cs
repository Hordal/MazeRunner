using System.Collections;
using UnityEngine;

public class CollapseTrap : MonoBehaviour
{
    [SerializeField] private bool unlocksExit;
    private FirstPersonController player;
    private bool inside;
    private bool sprinted;
    private bool triggered;

    public void SetUnlocksExit(bool value) => unlocksExit = value;

    private void OnTriggerEnter(Collider other)
    {
        if (triggered) return;
        player = other.GetComponent<FirstPersonController>();
        if (player == null) return;
        triggered = true;
        inside = true;
        GetComponent<Renderer>().material.color = new Color(1f, .12f, .02f);
        MazeGameManager.Instance.ShowEvent("바닥이 무너집니다! Shift로 빠르게 통과하세요.");
        StartCoroutine(CollapseRoutine());
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.GetComponent<FirstPersonController>() != null && player != null && player.IsSprinting) sprinted = true;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.GetComponent<FirstPersonController>() != null) inside = false;
    }

    private IEnumerator CollapseRoutine()
    {
        yield return new WaitForSeconds(.5f);
        if (player != null && !inside && sprinted)
        {
            MazeGameManager.Instance.ShowEvent("무너지는 바닥을 통과했습니다!");
            if (unlocksExit) { MazeGameManager.Instance.AllowFloor2Exit(); MazeGameManager.Instance.TryEscape(); }
        }
        else if (player != null)
        {
            MazeGameManager.Instance.ShowEvent("바닥이 무너졌습니다. 체크포인트로 돌아갑니다.");
            player.ReturnToCheckpoint();
        }
        Destroy(gameObject);
    }
}
