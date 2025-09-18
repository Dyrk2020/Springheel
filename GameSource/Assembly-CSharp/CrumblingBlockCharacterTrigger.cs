using UnityEngine;

public class CrumblingBlockCharacterTrigger : MonoBehaviour
{
	public CrumblingBlock crumblingblock;

	private void OnTriggerEnter2D(Collider2D c)
	{
		Character componentInParent = c.GetComponentInParent<Character>();
		if (componentInParent != null)
		{
			crumblingblock.CharacterTrigger(componentInParent);
		}
	}
}
