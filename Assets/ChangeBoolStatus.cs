using System.Collections;
using System.Collections.Generic;
using UnityEngine;




public class ChangeBoolStatus : StateMachineBehaviour
{
    public string boolName;
    public bool status;
    public bool resetOnExit;
    public float delay = .1f;

    Controller controller;

    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (controller == null)
            controller = animator.GetComponentInParent<Controller>();

        controller.StartCoroutine(DelayedOpen(delay, animator));
    }

    IEnumerator DelayedOpen(float d, Animator animator)
    {
        yield return new WaitForSeconds(d);
        animator.SetBool(boolName, status);
    }

    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    //override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    
    //}

    // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (resetOnExit)
            animator.SetBool(boolName, !status);
    }

    // OnStateMove is called right after Animator.OnAnimatorMove()
    //override public void OnStateMove(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    // Implement code that processes and affects root motion
    //}

    // OnStateIK is called right after Animator.OnAnimatorIK()
    //override public void OnStateIK(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    // Implement code that sets up animation IK (inverse kinematics)
    //}
}

