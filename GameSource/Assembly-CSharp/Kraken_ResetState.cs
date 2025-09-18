using UnityEngine;

public class Kraken_ResetState : StateMachineBehaviour
{
	private static readonly int AttackType = Animator.StringToHash("state");

	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		animator.SetInteger(AttackType, 0);
	}
}
