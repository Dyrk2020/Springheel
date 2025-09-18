using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PunchingPlant : ActiveBlock
{
	public float FearTime;

	public float PunchStrength;

	private Animator animator;

	private bool afraid;

	private bool punching;

	private bool mirrored;

	private List<Character> punchTargets = new List<Character>();

	private RaycastHit2D[] rayHits = new RaycastHit2D[10];

	public Transform rayCastOrigin;

	public LayerMask layerMask;

	private static TagComparer.Tag solidPlayerMask = (TagComparer.Tag)160;

	protected override void Start()
	{
		base.Start();
		animator = GetComponentInChildren<Animator>();
	}

	public override void Reset()
	{
		base.Reset();
		base.transform.localScale = originalScale;
		mirrored = originalScale.x < 0f;
	}

	private bool IsSolidPlayer(GameObject obj, CollisionTag collisionTag = null)
	{
		if (collisionTag != null)
		{
			return collisionTag.ContainsAllTags(solidPlayerMask);
		}
		return false;
	}

	public override void Pause()
	{
		base.Pause();
		animator.speed = 0f;
	}

	public override void Unpause()
	{
		base.Unpause();
		animator.speed = 1f;
	}

	private bool IsSolid(GameObject obj, CollisionTag collisionTag = null)
	{
		if (collisionTag != null)
		{
			return collisionTag.ExactTagMatch(TagComparer.Tag.Solid);
		}
		return false;
	}

	private void OnTriggerStay2D(Collider2D c)
	{
		if (punching || !isActive)
		{
			return;
		}
		Character componentInParent = c.gameObject.GetComponentInParent<Character>();
		if (componentInParent == null)
		{
			return;
		}
		CollisionTag component = c.GetComponent<CollisionTag>();
		if (!IsSolidPlayer(c.gameObject, component) || componentInParent.InBlackHole || !componentInParent.Enabled)
		{
			return;
		}
		Vector3 vector = componentInParent.transform.position - rayCastOrigin.position;
		if (!componentInParent.CrouchingDown)
		{
			vector += Vector3.up;
		}
		float num = Mathf.Atan2(vector.x, vector.y) * 57.29578f + base.transform.eulerAngles.z;
		bool flag = (num > 0f && num < 180f) || num < -180f || num > 360f;
		if ((afraid && flag != mirrored) || punchTargets.Contains(componentInParent))
		{
			return;
		}
		float num2 = vector.sqrMagnitude + 0.2f;
		bool queriesHitTriggers = Physics2D.queriesHitTriggers;
		Physics2D.queriesHitTriggers = false;
		int num3 = Physics2D.RaycastNonAlloc(rayCastOrigin.position, vector.normalized, rayHits, 10f, layerMask);
		Physics2D.queriesHitTriggers = queriesHitTriggers;
		for (int i = 0; i != num3; i++)
		{
			RaycastHit2D raycastHit2D = rayHits[i];
			float num4 = 10f * raycastHit2D.fraction;
			num4 *= num4;
			if (raycastHit2D.fraction != 0f && !(num4 > num2))
			{
				CollisionTag component2 = raycastHit2D.collider.GetComponent<CollisionTag>();
				if (IsSolid(raycastHit2D.collider.gameObject, component2))
				{
					return;
				}
			}
		}
		punchTargets.Add(componentInParent);
		if (afraid)
		{
			return;
		}
		afraid = true;
		mirrored = flag;
		animator.SetTrigger("Afraid");
		if (base.transform.parent != null && base.transform.parent.parent != null && base.transform.parent.parent.localScale.x < 0f)
		{
			if (mirrored)
			{
				base.transform.localScale = new Vector3(1f, 1f, 1f);
			}
			else
			{
				base.transform.localScale = new Vector3(-1f, 1f, 1f);
			}
		}
		else if (mirrored)
		{
			base.transform.localScale = new Vector3(-1f, 1f, 1f);
		}
		else
		{
			base.transform.localScale = new Vector3(1f, 1f, 1f);
		}
		StartCoroutine(beAfraid());
	}

	private void OnTriggerExit2D(Collider2D c)
	{
		Character componentInParent = c.gameObject.GetComponentInParent<Character>();
		if (!(componentInParent == null) && punchTargets.Contains(componentInParent))
		{
			punchTargets.Remove(componentInParent);
		}
	}

	private IEnumerator beAfraid()
	{
		float fearTimer = 0f;
		AkSoundEngine.PostEvent("SFX_Pieces_PunchingPlant_Afraid", base.gameObject);
		while (fearTimer < FearTime)
		{
			fearTimer += Time.deltaTime;
			yield return null;
		}
		animator.SetTrigger("Punch");
	}

	public void punch()
	{
		AkSoundEngine.PostEvent("SFX_Pieces_PunchingPlant_Punch", base.gameObject);
		Character[] array = punchTargets.ToArray();
		foreach (Character obj in array)
		{
			obj.KillCharacter("PunchingPlant", deathFreezeOn: false, placedByPlayerNumber);
			Vector3 vector = new Vector3(mirrored ? 1 : (-1), 1f, 0f);
			obj.AddImpulse(base.transform.rotation * vector * PunchStrength + Vector3.up * 0.25f * PunchStrength, 0.1f);
		}
	}

	public void reset()
	{
		afraid = false;
		punching = false;
		punchTargets.Clear();
		AkSoundEngine.PostEvent("SFX_Pieces_PunchingPlant_Idle", base.gameObject);
	}
}
