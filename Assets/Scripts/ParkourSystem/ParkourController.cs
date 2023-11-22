using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class ParkourController : MonoBehaviour
{
    [SerializeField]
    List<ParkourAction> parkourActions;

    [SerializeField]
    ParkourAction jumpDownAction;

    [SerializeField]
    float autoJumpHeightLimit = 1f;

    bool inAction;

    EnvironmentScanner environmentScanner;
    Animator animator;
    PlayerController playerController;

    public InputAction jumpAction;

    public void OnEnable()
    {
        jumpAction.Enable();
    }

    public void OnDisable()
    {
        jumpAction.Disable();
    }

    private void Awake()
    {
        environmentScanner = GetComponent<EnvironmentScanner>();
        animator = GetComponent<Animator>();
        playerController = GetComponent<PlayerController>();
    }

    private void Update()
    {
        var jumpInput = jumpAction.ReadValue<float>();
        var hitData = environmentScanner.ObstacleCheck();

        if (jumpInput > 0 && !inAction)
        {
            if (hitData.forwardHitFound)
            {
                foreach (var action in parkourActions)
                {
                    if (action.CheckIfPossible(hitData, transform))
                    {
                        StartCoroutine(DoParkourAction(action));
                        break;
                    }
                }
            }
        }

        if (playerController.IsOnLedge && !inAction && !hitData.forwardHitFound)
        {
            bool shouldJump = true;

            if (playerController.LedgeData.height > autoJumpHeightLimit && jumpInput <= 0)
            {
                shouldJump = false;
            }

            if (shouldJump && playerController.LedgeData.angle <= 50)
            {
                playerController.IsOnLedge = false;
                StartCoroutine(DoParkourAction(jumpDownAction));
            }
        }
    }

    IEnumerator DoParkourAction(ParkourAction action)
    {
        inAction = true;
        playerController.SetControl(false);

        animator.SetBool("mirrorAction", action.Mirror);
        animator.CrossFade(action.AnimName, 0.1f);
        // wait for the next frame
        yield return null;

        var animState = animator.GetNextAnimatorStateInfo(0);
        if (!animState.IsName(action.AnimName))
        {
            Debug.LogError("The parkour animation name is incorrect!");
        }

        float timer = 0f;
        // wait for the length of the animation
        while (timer <= animState.length)
        {
            timer += Time.deltaTime;

            // Rotate player to face the direction of the obstacle
            if (action.RotateToObstacle)
            {
                transform.rotation = Quaternion.RotateTowards(
                    transform.rotation,
                    action.TargetRotation,
                    playerController.RotationSpeed * Time.deltaTime
                );
            }

            if (action.EnableTargetMatching)
            {
                MatchTarget(action);
            }

            if (animator.IsInTransition(0) && timer > 0.5)
            {
                break;
            }

            yield return null;
        }

        yield return new WaitForSeconds(action.PostActionDelay);

        playerController.SetControl(true);
        inAction = false;
    }

    private void MatchTarget(ParkourAction action)
    {
        if (animator.IsInTransition(0))
        {
            return;
        }

        if (animator.isMatchingTarget)
        {
            return;
        }

        animator.MatchTarget(
            action.MatchPosition,
            transform.rotation,
            action.MatchBodyPart,
            new MatchTargetWeightMask(action.MatchPositionWeight, 0),
            action.MatchStartTime,
            action.MatchTargetTime
        );
    }
}
