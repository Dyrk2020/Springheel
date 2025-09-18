using UnityEngine;

public class AreaAudioSwitch : MonoBehaviour
{
	public string intialAudioEvent;

	public string areaAudioEvent;

	public bool characterInside;

	protected bool audioInArea;

	protected bool AudioUnderground
	{
		get
		{
			return audioInArea;
		}
		set
		{
			if (value)
			{
				if (!audioInArea)
				{
					AkSoundEngine.PostEvent(areaAudioEvent, base.gameObject);
					audioInArea = true;
				}
			}
			else if (audioInArea)
			{
				AkSoundEngine.PostEvent(intialAudioEvent, base.gameObject);
				audioInArea = false;
			}
		}
	}

	private void Start()
	{
		AkSoundEngine.PostEvent(intialAudioEvent, base.gameObject);
		audioInArea = false;
	}

	public void FixedUpdate()
	{
		if (characterInside)
		{
			AudioUnderground = true;
		}
		else
		{
			AudioUnderground = false;
		}
		characterInside = false;
	}

	public void OnTriggerStay2D(Collider2D c)
	{
		CollisionTag component = c.GetComponent<CollisionTag>();
		if (component != null && component.ContainsAnyTag(TagComparer.Tag.Player))
		{
			characterInside = true;
		}
	}
}
