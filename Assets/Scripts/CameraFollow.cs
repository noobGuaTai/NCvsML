using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform player1;
    public Transform player2;
    public float minSize = 5f;
    public float maxSize = 20f;
    public float distanceThreshold = 10f; // 当两个玩家之间的距离大于此值时，才开始拉远镜头
    public float zoomLimiter = 50f;
    public Vector3 offset;
    public float smoothTime = 0.5f;

    private Vector3 velocity;
    private Camera cam;

    void Start()
    {
        cam = GetComponent<Camera>();
        if (!cam.orthographic)
        {
            Debug.LogError("Camera must be orthographic.");
        }
    }

    void LateUpdate()
    {
        if (player1 == null || player2 == null)
            return;

        Move();
        Zoom();
    }

    void Move()
    {
        Vector3 centerPoint = GetCenterPoint();

        // 只在x轴上跟随玩家
        Vector3 newPosition = new Vector3(centerPoint.x + offset.x, transform.position.y, transform.position.z);

        transform.position = Vector3.SmoothDamp(transform.position, newPosition, ref velocity, smoothTime);
    }

    void Zoom()
    {
        float greatestDistance = GetGreatestDistance();

        if (greatestDistance < distanceThreshold)
        {
            cam.orthographicSize = Mathf.Lerp(cam.orthographicSize, minSize, Time.deltaTime);
        }
        else
        {
            float newSize = Mathf.Lerp(minSize, maxSize, (greatestDistance - distanceThreshold) / (zoomLimiter - distanceThreshold));
            cam.orthographicSize = Mathf.Lerp(cam.orthographicSize, newSize, Time.deltaTime);
        }
    }

    float GetGreatestDistance()
    {
        float distance = Vector3.Distance(player1.position, player2.position);
        return distance;
    }

    Vector3 GetCenterPoint()
    {
        Vector3 centerPoint = (player1.position + player2.position) / 2;
        return centerPoint;
    }
}
