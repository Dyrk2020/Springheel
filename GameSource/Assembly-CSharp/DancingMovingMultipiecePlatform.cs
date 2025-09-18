using UnityEngine;
using UnityEngine.Networking;

public class DancingMovingMultipiecePlatform : MultipieceBlock
{
	public DancingMoverRecordPlayer recordPlayer;

	protected Vector2 lastPosition;

	protected Vector2 velocity;

	private bool wasDancingLastFrame;

	public bool DancingCharacter { get; set; }

	public bool CharacterOnPlatform { get; set; }

	protected override void Awake()
	{
		base.Awake();
		InitializePhysicsModifiers();
	}

	protected override void Start()
	{
		base.Start();
		recordPlayer.Deactivate();
		lastPosition = recordPlayer.transform.position;
	}

	protected override void Update()
	{
		base.Update();
		UpdateRecordPlayerState();
		if (base.Active)
		{
			if (NetworkServer.active)
			{
				SynchronizeWithSurrogateHost();
			}
			else
			{
				SynchronizeWithSurrogateClient();
			}
		}
	}

	private void SynchronizeWithSurrogateHost()
	{
		if (NetSurrogate != null && recordPlayer != null)
		{
			NetSurrogate.transform.position = recordPlayer.transform.position;
		}
	}

	private void SynchronizeWithSurrogateClient()
	{
		if (NetSurrogate != null && recordPlayer != null)
		{
			recordPlayer.transform.position = NetSurrogate.transform.position;
		}
	}

	protected override void Act(float deltaTime)
	{
		if (!paused && !scoreboard && !(recordPlayer == null))
		{
			CalculateVelocity(deltaTime);
		}
	}

	private void CalculateVelocity(float deltaTime)
	{
		if (recordPlayer.transform.position.y >= lastPosition.y)
		{
			CalculateHorizontalVelocityOnly(deltaTime);
		}
		else
		{
			CalculateGlobalVelocity(deltaTime);
		}
	}

	private void CalculateHorizontalVelocityOnly(float deltaTime)
	{
		velocity.x = (recordPlayer.transform.position.x - lastPosition.x) / deltaTime;
		velocity.y = 0f;
		lastPosition = recordPlayer.transform.position;
	}

	private void CalculateGlobalVelocity(float deltaTime)
	{
		velocity = ((Vector2)recordPlayer.transform.position - lastPosition) / deltaTime;
		lastPosition = recordPlayer.transform.position;
	}

	private void UpdateRecordPlayerState()
	{
		if (DancingCharacter)
		{
			if (!wasDancingLastFrame)
			{
				ActivateRecordPlayer();
			}
		}
		else if (CharacterOnPlatform)
		{
			if (recordPlayer.movementState != DancingMoverRecordPlayer.DancingMoverState.STAND_BY)
			{
				StandByRecordPlayer();
			}
		}
		else if (wasDancingLastFrame || recordPlayer.movementState != DancingMoverRecordPlayer.DancingMoverState.RETURN)
		{
			DeactivateRecordPlayer();
		}
	}

	private void ActivateRecordPlayer()
	{
		wasDancingLastFrame = true;
		recordPlayer.Activate();
	}

	private void StandByRecordPlayer()
	{
		wasDancingLastFrame = false;
		recordPlayer.StandBy();
	}

	private void DeactivateRecordPlayer()
	{
		wasDancingLastFrame = false;
		recordPlayer.Deactivate();
	}

	public override void Reset()
	{
		base.Reset();
		velocity = Vector2.zero;
		if (recordPlayer != null)
		{
			recordPlayer.Reset();
			lastPosition = recordPlayer.transform.position;
		}
	}

	public override void Pause()
	{
		base.Pause();
		recordPlayer.Pause();
	}

	public override void Unpause()
	{
		base.Unpause();
		recordPlayer.Unpause();
	}

	private void InitializePhysicsModifiers()
	{
		pms = new PhysicsModifier[1];
		pms[0] = new PhysicsModifier(PhysicsModifier.ModType.BaseMotion, 0f, Vector2.zero, base.gameObject);
	}

	public override PhysicsModifier[] GetPhysicsModifiers()
	{
		UpdatePhysicsModifierData();
		return pms;
	}

	public override PhysicsModifier[] GetPhysicsModifier()
	{
		UpdatePhysicsModifierData();
		return pms;
	}

	private void UpdatePhysicsModifierData()
	{
		if (pms != null)
		{
			if (velocity.sqrMagnitude > 0f)
			{
				pms[0].Direction = velocity.normalized;
				pms[0].Magnitude = velocity.magnitude;
			}
			else
			{
				pms[0].Direction = Vector2.zero;
				pms[0].Magnitude = 0f;
			}
		}
	}
}
