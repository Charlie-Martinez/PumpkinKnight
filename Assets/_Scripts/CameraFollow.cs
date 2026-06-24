using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Target")]
    public Transform target; // Assign player here

    [Header("Settings")]
    public float smoothSpeed = 5f;
    public Vector3 offset = new Vector3(0, 0, -10);

    [Header("Clamping")]
    public bool clampToLevel = true;
    public Vector2 minClamp;
    public Vector2 maxClamp;

    void LateUpdate()
    {
        if (target == null) return;
        Vector3 desiredPosition = target.position + offset;
        if (clampToLevel)
        {
            desiredPosition.x = Mathf.Clamp(desiredPosition.x, minClamp.x, maxClamp.x);
            desiredPosition.y = Mathf.Clamp(desiredPosition.y, minClamp.y, maxClamp.y);
        }
        transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
    }
}
