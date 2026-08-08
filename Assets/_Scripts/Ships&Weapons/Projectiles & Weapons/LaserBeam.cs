using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class LaserBeam : MonoBehaviour
{
    private LineRenderer lineRenderer;
    private Transform startPoint;
    private Transform target;

    private void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.positionCount = 2;
        lineRenderer.useWorldSpace = true;
    }

    private void Update()
    {
        if (startPoint == null || target == null)
        {
            Destroy(gameObject);
            return;
        }

        lineRenderer.SetPosition(0, startPoint.position);
        lineRenderer.SetPosition(1, target.position);
    }

    public void Initialize(Transform newStartPoint, Transform newTarget)
    {
        startPoint = newStartPoint;
        target = newTarget;

        lineRenderer.SetPosition(0, startPoint.position);
        lineRenderer.SetPosition(1, target.position);
    }

    public void StopBeam()
    {
        Destroy(gameObject);
    }
}