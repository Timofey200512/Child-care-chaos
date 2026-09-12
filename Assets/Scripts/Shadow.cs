using UnityEngine;

public class Shadow : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform target;
    [SerializeField] private Collider targetCollider;

    [Header("Surface Detection")]
    [SerializeField] private LayerMask surfaceLayers = ~0;
    [SerializeField] private float castStartAboveTarget = 5f;
    [SerializeField] private float maxCastDistance = 100f;
    [SerializeField] private float shadowVisualLift = 0.01f;
    [SerializeField] private float maxAcceptedSurfaceHeightAboveFeet = 0.2f;

    private readonly RaycastHit[] hitsBuffer = new RaycastHit[32];

    private void Awake()
    {
        if (targetCollider == null && target != null)
        {
            targetCollider = target.GetComponent<Collider>();
        }
    }

    private void Update()
    {
        Vector3 targetPosition = target.position;
        float targetFeetY = targetCollider.bounds.min.y;

        Vector3 rayOrigin =
            new Vector3(targetPosition.x, targetFeetY + castStartAboveTarget, targetPosition.z);

        int hitCount = Physics.RaycastNonAlloc(
            rayOrigin,
            Vector3.down,
            hitsBuffer,
            maxCastDistance,
            surfaceLayers,
            QueryTriggerInteraction.Ignore);

        bool foundSurface = false;
        float highestSurfaceY = float.NegativeInfinity;

        for (int i = 0; i < hitCount; i++)
        {
            Collider hitCollider = hitsBuffer[i].collider;
            if (!hitCollider)
            {
                continue;
            }

            if (targetCollider && hitCollider == targetCollider)
            {
                continue;
            }

            if (hitCollider.transform.IsChildOf(target))
            {
                continue;
            }

            float hitY = hitsBuffer[i].point.y;
            if (hitY <= targetFeetY + maxAcceptedSurfaceHeightAboveFeet && hitY > highestSurfaceY)
            {
                highestSurfaceY = hitY;
                foundSurface = true;
            }
        }

        if (!foundSurface)
        {
            return;
        }
        print("updating shadow position");
        transform.position = new Vector3(
            target.position.x,
            highestSurfaceY + shadowVisualLift,
            target.position.z);
    }
}
