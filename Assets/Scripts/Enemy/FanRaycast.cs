using UnityEngine;

public class FanRaycast : MonoBehaviour
{
    public float radius = 5f; // Radius of the fan
    public float angle = 60f; // Angle of the fan in degrees
    public int res = 10;
    public LayerMask interactionLayer; // Layer mask for objects to interact with

    private void Update()
    {
        // Calculate fan points
        float halfAngle = angle * 0.5f;
        float angleStep = Mathf.PI * halfAngle / 180f / 10f;

        for (int i = -res; i <= res; i++)
        {
            float t = i / (float)res;
            float currentAngle = t * halfAngle * Mathf.Deg2Rad;
            Vector3 direction = Quaternion.Euler(0f, currentAngle * Mathf.Rad2Deg, 0f) * transform.forward;

            // Perform raycast
            RaycastHit hit;
            if (Physics.Raycast(transform.position, direction, out hit, radius, interactionLayer))
            {
                // Perform interaction with the detected object
                GameObject detectedObject = hit.collider.gameObject;
                InteractionAction(detectedObject);
            }
        }
    }


    private void InteractionAction(GameObject interactedObject)
    {
        // Implement interaction logic here
        Debug.Log("Interacting with: " + interactedObject.name);
    }

    private void OnDrawGizmos()
    {
        // Visualize the fan-shaped area using Gizmos
        Vector3 direction = transform.forward;

        float halfAngle = angle * 0.5f;
        float angleStep = Mathf.PI * halfAngle / 180f / 10f;

        for (int i = -res; i <= res; i++)
        {
            float t = i / (float)res;
            float currentAngle = t * halfAngle * Mathf.Deg2Rad;
            Vector3 offset = Quaternion.Euler(0f, currentAngle * Mathf.Rad2Deg, 0f) * direction * radius;
            Vector3 start = transform.position;
            Vector3 end = transform.position + offset;
            Gizmos.DrawLine(start, end);
        }
    }
}
