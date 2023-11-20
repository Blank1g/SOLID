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
    LayerMask obstacleLayer;

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
}

public struct ObstacleHitData
{
    public bool forwardHitFound;
    public bool heightHitFound;
    public RaycastHit forwardHit;
    public RaycastHit heightdHit;
}
