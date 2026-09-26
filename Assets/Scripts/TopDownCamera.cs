using UnityEngine;

public class TopDownCamera : MonoBehaviour
{
    public Transform target;         
    public Vector3 offset = new Vector3(0f, 15f, -8f); 
    public float smoothSpeed = 10f;

    void LateUpdate()
    {
        if (target == null) return;

        Vector3 desiredPosition = target.position + offset;
        transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
    }
}
