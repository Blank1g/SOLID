using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EnvironmentScanner : MonoBehaviour
{
    [SerializeField]
    Vector3 forwardRayOffset = new Vector3(0, 2.5f, 0);

    [SerializeField]
    float forwardRayLength = 0.8f;

    [SerializeField]
    LayerMask heightRayLength = 5;

    [SerializeField]
    float ledgeRayLength = 10f;

    [SerializeField]
    LayerMask obstacleLayer;

    [SerializeField]
    float ledgeHeightThreshold = 0.75f;

    public ObstacleHitData ObstacleCheck()
    {
        var hitData = new ObstacleHitData();

        var forwardOrigin = transform.position + forwardRayOffset;
        hitData.forwardHitFound = Physics.Raycast(
            forwardOrigin,
            transform.forward,
            out hitData.forwardHit,
            forwardRayLength,
            obstacleLayer
        );

        Debug.DrawRay(
            forwardOrigin,
            transform.forward * forwardRayLength,
            hitData.forwardHitFound ? Color.green : Color.white
        );

        if (hitData.forwardHitFound)
        {
            var heightOrigin = hitData.forwardHit.point + Vector3.up * heightRayLength;
            hitData.heightHitFound = Physics.Raycast(
                heightOrigin,
                Vector3.down,
                out hitData.heightdHit,
                heightRayLength,
                obstacleLayer
            );

            Debug.DrawRay(
                heightOrigin,
                Vector3.down * heightRayLength,
                hitData.heightHitFound ? Color.green : Color.white
            );
        }

        return hitData;
    }

    public bool LedgeCheck(Vector3 moveDir, out LedgeData ledgeData)
    {
        ledgeData = new LedgeData();

        if (moveDir == Vector3.zero)
        {
            return false;
        }

        // The distance character should stop from the ledge
        float originOffset = 0.5f;
        var origin = transform.position + moveDir * originOffset + Vector3.up;

        if (
            PhysicsUtil.ThreeRaycasts(
                origin,
                Vector3.down,
                0.25f,
                transform,
                out List<RaycastHit> hits,
                ledgeRayLength,
                obstacleLayer,
                true
            )
        )
        {
            var validHits = hits.Where(h => transform.position.y - h.point.y > ledgeHeightThreshold)
                .ToList();

            if (validHits.Count > 0)
            {
                var surfaceRayOrigin = validHits[0].point;
                surfaceRayOrigin.y = transform.position.y - 0.1f;

                if (
                    Physics.Raycast(
                        surfaceRayOrigin,
                        transform.position - surfaceRayOrigin,
                        out RaycastHit surfaceHit,
                        2,
                        obstacleLayer
                    )
                )
                {
                    Debug.DrawLine(surfaceRayOrigin, transform.position, Color.red);

                    float height = transform.position.y - validHits[0].point.y;

                    ledgeData.angle = Vector3.Angle(transform.forward, surfaceHit.normal);
                    ledgeData.height = height;
                    ledgeData.surfaceHit = surfaceHit;

                    return true;
                }
            }
        }

        Debug.DrawRay(origin, Vector3.down * ledgeRayLength, Color.white);
        return false;
    }
}

public struct ObstacleHitData
{
    public bool forwardHitFound;
    public bool heightHitFound;
    public RaycastHit forwardHit;
    public RaycastHit heightdHit;
}

public struct LedgeData
{
    public float height;
    public float angle;
    public RaycastHit surfaceHit;
}
