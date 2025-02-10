using UnityEngine;
/// <summary>
/// Resetuje Triger vv animatore aby sa zabranilo nechcenemu spravaniu
/// </summary>
public class ResetTrigger : StateMachineBehaviour
{
    [SerializeField] string resetTrigger = "";
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) => animator.ResetTrigger(resetTrigger);
}
