using System.Collections;
using UnityEngine;

public class AnimationTriggersSoundLimited : AnimationTriggersSound
{
	public bool limitRecurrence;

	public float doNotPlayTime;

	public static bool DonotPlay;

	private void playSound()
	{
		if (!limitRecurrence)
		{
			if (!EventName.Equals(""))
			{
				AkSoundEngine.PostEvent(EventName, base.gameObject);
			}
		}
		else if (!DonotPlay)
		{
			AkSoundEngine.PostEvent(EventName, base.gameObject);
			DonotPlay = true;
			StartCoroutine(WaitTillNextPlay());
		}
	}

	private IEnumerator WaitTillNextPlay()
	{
		yield return new WaitForSeconds(doNotPlayTime);
		DonotPlay = false;
	}
}
