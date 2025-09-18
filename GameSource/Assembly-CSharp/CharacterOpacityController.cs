using UnityEngine;

public class CharacterOpacityController : MonoBehaviour
{
	public Character character;

	public float currentOpacity = 1f;

	public float transitionSpeed = 4f;

	public int invisibleFramesThreshold = 3;

	public int invisibleFrames;

	public float forceVisibleTime;

	private void Update()
	{
		float num = 1f;
		if (forceVisibleTime > 0f)
		{
			forceVisibleTime -= Time.deltaTime;
			if (forceVisibleTime < 0f)
			{
				forceVisibleTime = 0f;
			}
		}
		if (character.Invisible && forceVisibleTime <= 0f)
		{
			invisibleFrames++;
		}
		else
		{
			invisibleFrames = 0;
		}
		if (invisibleFrames > invisibleFramesThreshold)
		{
			num = 0f;
		}
		else if (character.isGhost && !character.isZombie)
		{
			num = GameSettings.GetInstance().ghostAlpha;
		}
		if (currentOpacity == num)
		{
			return;
		}
		if (currentOpacity > num)
		{
			currentOpacity -= transitionSpeed * Time.deltaTime;
			if (currentOpacity <= num)
			{
				currentOpacity = num;
			}
		}
		else if (currentOpacity < num)
		{
			currentOpacity += transitionSpeed * Time.deltaTime;
			if (currentOpacity >= num)
			{
				currentOpacity = num;
			}
		}
	}

	public void OnStartPlayPhase()
	{
		forceVisibleTime += 0.5f;
	}
}
