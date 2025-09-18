using System.Collections.Generic;
using UnityEngine;

public class PunchingBlockTrigger : MonoBehaviour
{
	public PunchingBlock punchingBlock;

	public Animator triggerAnimator;

	private int lastNumCharacters;

	private float triggeredByClient;

	private bool animatorOn;

	private HashSet<Collider2D> characterColliders = new HashSet<Collider2D>();

	private bool RecentlyTriggeredByClient => triggeredByClient > 0f;

	public bool TriggeredByLocalPlayer => lastNumCharacters > 0;

	public void SetPushed(bool pushed)
	{
		triggerAnimator.SetBool("Pushed", pushed);
		animatorOn = pushed;
		if (pushed)
		{
			AkSoundEngine.PostEvent("SFX_Pieces_Boxing_Glove_Trigger", punchingBlock.gameObject);
		}
	}

	public void Reset()
	{
		SetPushed(pushed: false);
		characterColliders.Clear();
		lastNumCharacters = 0;
		triggeredByClient = 0f;
	}

	private void OnTriggerEnter2D(Collider2D collider)
	{
		if (collider.GetComponent<Cursor>() == null)
		{
			characterColliders.Add(collider);
		}
	}

	private void OnTriggerExit2D(Collider2D collider)
	{
		characterColliders.Remove(collider);
	}

	private void FixedUpdate()
	{
		if (triggeredByClient > 0f)
		{
			triggeredByClient -= Time.deltaTime;
			if (triggeredByClient < 0f)
			{
				triggeredByClient = 0f;
			}
		}
		int count = characterColliders.Count;
		if (lastNumCharacters == 0 && count > 0)
		{
			SetPushed(pushed: true);
			punchingBlock.OnTriggerTouched();
		}
		if (animatorOn)
		{
			if (!RecentlyTriggeredByClient && count == 0)
			{
				SetPushed(pushed: false);
			}
		}
		else if (RecentlyTriggeredByClient)
		{
			SetPushed(pushed: true);
		}
		lastNumCharacters = count;
	}

	public void OnClientTouchedTrigger()
	{
		triggeredByClient = 0.1f;
	}
}
