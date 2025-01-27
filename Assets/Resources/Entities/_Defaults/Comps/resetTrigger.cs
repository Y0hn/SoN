using UnityEngine;

public class ResetTrigger : StateMachineBehaviour
{
    [SerializeField] string resetTrigger = "";
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        animator.ResetTrigger(resetTrigger);
    }
}
