using UnityEngine;

[RequireComponent(typeof(LineRenderer))]

public class LaserBeam : MonoBehaviour
{
    [SerializeField] private float beamDuration = 0.1f;

    private LineRenderer lineRenderer;
    private Transform startPoint;
    private Transform target;

    private float remainingDuration;
    private bool hasBeenInitialized;

    private void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();

        lineRenderer.positionCount = 2;
        lineRenderer.useWorldSpace = true;
        lineRenderer.enabled = false;
    }

    public void Initialize(Transform newStartingPoint, Transform newTarget, float newDuration)
    {
        if (newStartingPoint == null || newTarget == null)
        {
            Destroy(gameObject);
            return;
        }

        startPoint = newStartingPoint;
        target = newTarget;

        remainingDuration = newDuration > 0f ? newDuration : beamDuration;

        hasBeenInitialized = true;
        lineRenderer.enabled = true;

        UpdateBeamPosition();
    }

    private void Update()
    {
        if (!hasBeenInitialized)
        {
            return;
        }

        if (startPoint == null || target == null)
        {
            Destroy(gameObject);
            return;
        }

        UpdateBeamPosition();

        remainingDuration -= Time.deltaTime;

        if (remainingDuration <= 0f)
        {
            Destroy(gameObject);
        }
    }


    private void UpdateBeamPosition()
    {
        lineRenderer.SetPosition(0, startPoint.position);

        lineRenderer.SetPosition(1, target.position);
    }



}
