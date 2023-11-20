using UnityEngine;

[CreateAssetMenu(menuName = "Parkour System/New Parkour Action")]
public class ParkourAction : ScriptableObject
{
    [SerializeField]
    string animName;

    [SerializeField]
    string obstacleTag;

    [SerializeField]
    float minHeight;

    [SerializeField]
    float maxHeight;

    [SerializeField]
    bool rotateToObstacle;

    [SerializeField]
    float postActionDelay;

    [Header("Target Matching")]
    [SerializeField]
    bool enableTargetMatching = true;

    [SerializeField]
    protected AvatarTarget matchBodyPart;

    [SerializeField]
    float matchStartTime;

    [SerializeField]
    float matchTargetTime;

    [SerializeField]
    Vector3 matchPositionWeight = new Vector3(0, 1, 0);

    public string AnimName => animName;
    public bool RotateToObstacle => rotateToObstacle;
    public bool EnableTargetMatching => enableTargetMatching;
    public AvatarTarget MatchBodyPart => matchBodyPart;
    public float MatchStartTime => matchStartTime;
    public float MatchTargetTime => matchTargetTime;
    public Vector3 MatchPositionWeight => matchPositionWeight;
    public float PostActionDelay => postActionDelay;

    public Quaternion TargetRotation { get; set; }
    public Vector3 MatchPosition { get; set; }
    public bool Mirror { get; set; }

    public virtual bool CheckIfPossible(ObstacleHitData hitData, Transform player)
    {
        // Check if the forward ray hit an obstacle
        if (!string.IsNullOrEmpty(obstacleTag) && hitData.forwardHit.transform.tag != obstacleTag)
        {
            return false;
        }

        // Height Tag
        float height = hitData.heightdHit.point.y - player.position.y;
        if (height < minHeight || height > maxHeight)
        {
            return false;
        }

        if (rotateToObstacle)
        {
            TargetRotation = Quaternion.LookRotation(-hitData.forwardHit.normal);
        }

        if (enableTargetMatching)
        {
            MatchPosition = hitData.heightdHit.point;
        }

        return true;
    }
}
