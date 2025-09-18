using System.Collections;
using UnityEngine;

public class AnimationTriggersSound : MonoBehaviour
{
	public bool playSoundAsParent;

	public string EventName;

	public string EventNameB;

	public string EventNameC;

	public string EventNameD;

	public string EventNameE;

	public string EventNameF;

	public GameObject Go;

	public GameObject GoB;

	public GameObject GoC;

	public GameObject GoD;

	public GameObject GoE;

	public GameObject GoF;

	public float SuppressSoundsAfterLoadForSeconds;

	public bool canPlaySounds;

	public bool SoundToggle;

	private bool FirstSoundPlayed;

	private void Start()
	{
		if (SuppressSoundsAfterLoadForSeconds > 0f)
		{
			StartCoroutine(AllowSoundsAfterWait());
		}
		else
		{
			canPlaySounds = true;
		}
	}

	private IEnumerator AllowSoundsAfterWait()
	{
		yield return new WaitForSeconds(SuppressSoundsAfterLoadForSeconds);
		canPlaySounds = true;
	}

	private void playSound()
	{
		playSound(EventName, Go);
	}

	private void playSoundB()
	{
		playSound(EventNameB, GoB);
	}

	private void playSoundC()
	{
		playSound(EventNameC, GoC);
	}

	private void playSoundD()
	{
		playSound(EventNameD, GoD);
	}

	private void playSoundE()
	{
		playSound(EventNameE, GoE);
	}

	private void playSoundF()
	{
		playSound(EventNameF, GoF);
	}

	private void playSound(string eventName, GameObject go)
	{
		if (!canPlaySounds)
		{
			return;
		}
		if (SoundToggle)
		{
			if (FirstSoundPlayed)
			{
				return;
			}
			FirstSoundPlayed = true;
		}
		if (!EventName.Equals(""))
		{
			if (playSoundAsParent)
			{
				AkSoundEngine.PostEvent(eventName, base.gameObject.transform.parent.gameObject);
			}
			else if (go == null)
			{
				AkSoundEngine.PostEvent(eventName, base.gameObject);
			}
			else
			{
				AkSoundEngine.PostEvent(eventName, go);
			}
		}
	}
}
