using System.Collections.Generic;
using UnityEngine;

public class Spring : Placeable
{
	public CheckColliding Trigger;

	public float Strength;

	public Vector2 Direction;

	public Animator SpringAnimator;

	private Dictionary<Character, float> lastPushed = new Dictionary<Character, float>();

	private const float pushDelay = 0.2f;

	protected override void Start()
	{
		base.Start();
	}

	protected override void FixedUpdate()
	{
		base.FixedUpdate();
		if (NetSurrogate != null && NetSurrogate.TriggerVal)
		{
			SpringAnimator.SetTrigger("Triggered");
			AkSoundEngine.PostEvent("SFX_Pieces_Spring", base.gameObject);
		}
	}

	private void OnCollisionEnter2D(Collision2D c)
	{
		OnCollisionStay2D(c);
	}

	private void OnCollisionStay2D(Collision2D c)
	{
		Character component = c.gameObject.GetComponent<Character>();
		if (component == null)
		{
			return;
		}
		ContactPoint2D[] contacts = c.contacts;
		for (int i = 0; i < contacts.Length; i++)
		{
			ContactPoint2D contactPoint2D = contacts[i];
			CheckColliding component2 = contactPoint2D.collider.GetComponent<CheckColliding>();
			CheckColliding component3 = contactPoint2D.otherCollider.GetComponent<CheckColliding>();
			if (component2 != Trigger && component3 != Trigger)
			{
				return;
			}
		}
		bool flag = false;
		if (Vector3.Dot(component.transform.position - base.transform.position, base.transform.up) > 0f)
		{
			float value = 0f;
			if (lastPushed.TryGetValue(component, out value))
			{
				if (Time.timeSinceLevelLoad - value > 0.2f)
				{
					flag = true;
					lastPushed[component] = Time.timeSinceLevelLoad;
				}
			}
			else
			{
				flag = true;
				lastPushed.Add(component, Time.timeSinceLevelLoad);
			}
		}
		if (flag)
		{
			if (component.AddImpulse(base.transform.rotation * Direction * Strength + Vector3.up * 0.25f * Strength, 0.1f, resetAirJumps: true))
			{
				AkSoundEngine.PostEvent("SFX_Pieces_Spring", base.gameObject);
				StatTracker.Instance.GetSaveFileDataForLocalPlayer(component.networkNumber, fallback: true)?.IncrementStat("SpringBounces");
			}
			SpringAnimator.SetTrigger("Triggered");
			if (NetSurrogate != null)
			{
				NetSurrogate.TriggerVal = true;
			}
		}
	}
}
