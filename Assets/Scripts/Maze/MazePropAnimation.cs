using UnityEngine;

/// <summary>
/// Small visual motion shared by pickup and portal props.  It does not change
/// game collision; it only makes important objects readable at a distance.
/// </summary>
public sealed class MazePropAnimation : MonoBehaviour
{
    public bool rotate = true;
    public bool bob = true;
    public float rotationSpeed = 55f;
    public float bobHeight = 0.1f;
    public float bobSpeed = 2.1f;

    private Vector3 startLocalPosition;
    private float phase;

    private void Awake()
    {
        startLocalPosition = transform.localPosition;
        phase = Random.value * Mathf.PI * 2f;
    }

    private void Update()
    {
        if (rotate) transform.Rotate(0f, rotationSpeed * Time.deltaTime, 0f, Space.Self);
        if (bob)
        {
            Vector3 position = startLocalPosition;
            position.y += Mathf.Sin(Time.time * bobSpeed + phase) * bobHeight;
            transform.localPosition = position;
        }
    }
}
