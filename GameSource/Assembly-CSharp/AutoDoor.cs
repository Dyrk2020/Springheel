using System.Collections;
using UnityEngine;

public class AutoDoor : ActiveBlock
{
	public Collider2D DoorCollider;

	public CheckColliding doorColliderCMC;

	public Animator DoorAnimator;

	public float Interval;

	private float timer;

	private bool open;

	private bool real = true;

	private bool colliderEnabled = true;

	public Transform pushOutPointLeft;

	public bool timed = true;

	public CheckColliding movementDetector;

	public int beamArtSpriteIndex;

	public Collider2D doorHazard;

	public float DoorHazardDelay;

	private bool doorHazardEnabled;

	protected override void Start()
	{
		base.Start();
		DoorAnimator.SetBool("Open", value: false);
		if (movementDetector != null)
		{
			movementDetector.onCollidingCharactersUpdated = OnDetectedCharactersUpdated;
		}
	}

	protected override void Act(float deltaTime)
	{
		if (timed)
		{
			timer += deltaTime;
			float num = Interval / Modifiers.GetInstance().PlatformMoveSpeed;
			if (timer >= num)
			{
				OnTriggered();
				timer = 0f;
			}
		}
		else
		{
			UpdateCharacterDetection(deltaTime);
			if (NetSurrogate != null && NetSurrogate.TriggerVal)
			{
				OnTriggeredByNetwork();
			}
		}
	}

	private void UpdateCharacterDetection(float deltaTime)
	{
		bool flag = HasAnyCharacters();
		if (open)
		{
			if (flag)
			{
				timer = 0f;
			}
			else
			{
				timer += deltaTime;
			}
			float num = Interval / Modifiers.GetInstance().PlatformMoveSpeed;
			if (timer >= num && !flag)
			{
				OnTriggered();
				timer = 0f;
			}
		}
		else if (flag)
		{
			OnTriggered();
			timer = 0f;
		}
	}

	private void OnDetectedCharactersUpdated()
	{
		UpdateCharacterDetection(0f);
	}

	private bool HasAnyCharacters()
	{
		if (movementDetector.CollidingObject != null)
		{
			Character componentInParent = movementDetector.CollidingObject.GetComponentInParent<Character>();
			if (componentInParent != null)
			{
				Collider2D component = movementDetector.CollidingObject.GetComponent<Collider2D>();
				if (componentInParent.CrouchingDown && (component == componentInParent.hazardHeadcollider || component == componentInParent.headCollider))
				{
					if (!movementDetector.enabled)
					{
						movementDetector.CollidingObject = null;
					}
					return false;
				}
				if (component == componentInParent.coinGrabber)
				{
					return false;
				}
				return true;
			}
		}
		return false;
	}

	private bool HasMovingCharacter()
	{
		foreach (Character collidingCharacter in movementDetector.CollidingCharacters)
		{
			bool flag = false;
			switch (collidingCharacter.CurrentAnim)
			{
			case Character.AnimState.RUN:
			case Character.AnimState.WALK:
			case Character.AnimState.JUMP:
			case Character.AnimState.SLIDE:
			case Character.AnimState.WIN:
				flag = true;
				break;
			}
			if (flag || !collidingCharacter.OnGround || Mathf.Abs(collidingCharacter.Velocity.x) > 0.01f)
			{
				return true;
			}
		}
		return false;
	}

	private void OnTriggeredByNetwork()
	{
		if (!open)
		{
			OnTriggered();
		}
	}

	private void OnTriggered()
	{
		open = !open;
		DoorAnimator.SetBool("Open", open);
		if (!timed && open)
		{
			disableDoorCollider();
		}
		if (real)
		{
			if (open)
			{
				PlayOpenSound();
			}
			else if (timed)
			{
				PlayCloseSound();
			}
		}
	}

	public void PlayOpenSound()
	{
		AkSoundEngine.PostEvent(timed ? "SFX_Pieces_Automatic_Door_Open" : "SFX_Pieces_OneWayDoor_Open", base.gameObject);
	}

	public void PlayCloseSound()
	{
		AkSoundEngine.PostEvent(timed ? "SFX_Pieces_Automatic_Door_Close" : "SFX_Pieces_OneWayDoor_Close", base.gameObject);
	}

	public override void Reset()
	{
		base.Reset();
		timer = 0f;
		DoorAnimator.SetBool("Open", value: false);
		open = false;
		if (timed)
		{
			PlayCloseSound();
		}
	}

	public override void Disable()
	{
		base.Disable();
		real = false;
	}

	public override void Enable()
	{
		base.Enable();
		real = true;
		if (!open)
		{
			DoorCollider.enabled = !open;
		}
	}

	public void enableDoorCollider()
	{
		if (real && doorColliderCMC.CurrentPhase == ColliderModeEnum.RunPhase)
		{
			SetDoorColliderEnabled(val: true);
		}
	}

	public void disableDoorCollider()
	{
		if (real && doorColliderCMC.CurrentPhase == ColliderModeEnum.RunPhase)
		{
			SetDoorColliderEnabled(val: false);
		}
	}

	private void SetDoorColliderEnabled(bool val)
	{
		if (colliderEnabled == val && DoorCollider.enabled == val)
		{
			return;
		}
		colliderEnabled = val;
		DoorCollider.enabled = val;
		if (movementDetector != null)
		{
			movementDetector.enabled = !val;
		}
		if (doorHazard != null)
		{
			doorHazardEnabled = val;
			if (val)
			{
				StartCoroutine(delayDoorHazard());
			}
			else
			{
				doorHazard.enabled = doorHazardEnabled;
			}
		}
	}

	private IEnumerator delayDoorHazard()
	{
		if (DoorHazardDelay > 0f)
		{
			yield return new WaitForSeconds(DoorHazardDelay);
		}
		if (doorHazard != null)
		{
			doorHazard.enabled = doorHazardEnabled;
		}
	}

	public void PushOutCharacters()
	{
		if (!real)
		{
			return;
		}
		foreach (Character item in Character.GetCharactersInCollider(DoorCollider))
		{
			if (item.hasAuthority && !item.pushedByDoorThisFrame)
			{
				PushOutCharacter(item);
			}
		}
	}

	private void PushOutCharacter(Character chr)
	{
		Vector3 vector = Mathf.Sign(pushOutPointLeft.localPosition.x) * base.transform.right * base.transform.localScale.x;
		if (Mathf.Abs(Vector3.Dot(vector, Vector3.up)) < 0.5f)
		{
			float num = Vector3.Dot(chr.transform.position - pushOutPointLeft.position, vector);
			if (num < 0f)
			{
				Vector3 vector2 = num * vector;
				chr.PositionCharacter(chr.transform.position - vector2);
				chr.pushedByDoorThisFrame = true;
			}
		}
	}

	private void OnDrawGizmos()
	{
		Gizmos.DrawLine(base.transform.position, base.transform.position + new Vector3(0f, 1f, 0f));
	}

	public void OnProjectileTouchedTrigger()
	{
		if (!timed && base.Active)
		{
			timer = 0f;
			OnTriggeredByNetwork();
		}
	}

	public override void Pause()
	{
		base.Pause();
		DoorAnimator.speed = 0f;
	}

	public override void Unpause()
	{
		base.Unpause();
		DoorAnimator.speed = 1f;
	}

	public override void Tint()
	{
		base.Tint();
		if (!timed && (bombTints > 0 || pickedUp || (ParentPiece != null && ParentPiece.PickedUp) || HoveredCursors.Count > 0))
		{
			ArtSprites[beamArtSpriteIndex].color = initialColors[beamArtSpriteIndex];
		}
	}
}
