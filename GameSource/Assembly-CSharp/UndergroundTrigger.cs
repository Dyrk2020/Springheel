using UnityEngine;

public class UndergroundTrigger : MonoBehaviour
{
	public float fadeSpeed = 1f;

	public bool characterInside;

	[Range(0f, 1f)]
	public float alpha;

	protected bool audioUnderGround;

	protected bool AudioUnderground
	{
		get
		{
			return audioUnderGround;
		}
		set
		{
			if (value)
			{
				if (!audioUnderGround)
				{
					AkSoundEngine.PostEvent("SFX_Lobby_Challenge_UnMuffle", base.gameObject);
					audioUnderGround = true;
				}
			}
			else if (audioUnderGround)
			{
				AkSoundEngine.PostEvent("SFX_Lobby_Challenge_Muffle", base.gameObject);
				audioUnderGround = false;
			}
		}
	}

	private void Start()
	{
		alpha = 1f;
		audioUnderGround = false;
	}

	public void FixedUpdate()
	{
		if (characterInside)
		{
			alpha = Mathf.MoveTowards(alpha, 0f, fadeSpeed * Time.fixedDeltaTime);
			AudioUnderground = true;
		}
		else
		{
			alpha = Mathf.MoveTowards(alpha, 1f, fadeSpeed * Time.fixedDeltaTime);
			AudioUnderground = false;
		}
		characterInside = false;
	}

	private void OnDestroy()
	{
		AkSoundEngine.PostEvent("SFX_Lobby_Challenge_UnMuffle", base.gameObject);
		AkSoundEngine.PostEvent("SFX_Lobby_Challenge_Muffle", base.gameObject);
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
