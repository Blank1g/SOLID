using UnityEngine;

public class ControlStoppingAction : StateMachineBehaviour
{
    // We can attach this to any animation state to stop the player from moving
    PlayerController player;

    public override void OnStateEnter(
        Animator animator,
        AnimatorStateInfo stateInfo,
        int layerIndex
    )
    {
        if (player == null)
        {
            player = animator.GetComponent<PlayerController>();
        }
        player.HasControl = false;
    }

    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        player.HasControl = true;
    }
}
