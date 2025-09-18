using UnityEngine;

public class CharacterFallingSound : StateMachineBehaviour
{
	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		Character componentInParent = animator.GetComponentInParent<Character>();
		if (componentInParent != null)
		{
			AkSoundEngine.PostEvent("SFX_" + componentInParent.CharacterSFXNameNoCustom + "_Falling", componentInParent.gameObject);
		}
	}
}
