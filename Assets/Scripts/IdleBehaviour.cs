using UnityEngine;

public class IdleBehaviour : StateMachineBehaviour
{

    [SerializeField] private float idleTime;

    [SerializeField] private float numberOfIdleAnims;

    private float _idleTime;
    private bool animLoop = false;   
    private float randomAnimationIndex; 

    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
       ResetIdle();
    }

    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if(!animLoop){
            _idleTime += Time.deltaTime;
            if(_idleTime > idleTime && stateInfo.normalizedTime % 1 < 0.02f){
                animLoop = true;
                randomAnimationIndex = Random.Range(1f, numberOfIdleAnims+1f); 
            }
        }else if(stateInfo.normalizedTime % 1 > 0.98f){
            ResetIdle();
        }
        animator.SetFloat("IdleAnim",randomAnimationIndex, 0.2f, Time.deltaTime);
    }

    private void ResetIdle(){
        _idleTime = 0;
        animLoop = false;
        randomAnimationIndex = 0;
    }

    // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    //override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    
    //}

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
