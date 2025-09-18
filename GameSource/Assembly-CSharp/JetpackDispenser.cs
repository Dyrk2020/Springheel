using UnityEngine;

public class JetpackDispenser : ActiveBlock
{
	public int JetpacksPerRound;

	public bool InfiniteJetpacks;

	public float JetpackCooldown;

	public Jetpack JetpackPrefab;

	public Transform JetpackSpawnPoint;

	public Animator animator;

	public SpriteRenderer FakeJetpack;

	private float timeSinceLastJetpack;

	private int jetpacksLeft;

	private Jetpack currentJetpack;

	private int dispenseHash;

	private int resetHash;

	private bool firstJetPack = true;

	private int lastNetIDofJetpack;

	protected override void Awake()
	{
		base.Awake();
		dispenseHash = Animator.StringToHash("Dispense");
		resetHash = Animator.StringToHash("Reset");
	}

	protected override void Act(float deltaTime)
	{
		base.Act(deltaTime);
		if (NetSurrogate != null)
		{
			if (NetSurrogate.hasAuthority)
			{
				if ((!(currentJetpack == null) && !currentJetpack.MarkedForDestruction) || (!InfiniteJetpacks && jetpacksLeft <= 0))
				{
					return;
				}
				timeSinceLastJetpack += deltaTime;
				if (timeSinceLastJetpack >= JetpackCooldown)
				{
					currentJetpack = Object.Instantiate(JetpackPrefab, JetpackSpawnPoint.transform.position, base.transform.rotation);
					currentJetpack.transform.parent = base.transform;
					currentJetpack.IsSaveable = false;
					currentJetpack.GenerateIDOnPick(currentJetpack.ID, -1);
					currentJetpack.Place(0);
					currentJetpack.transform.localRotation = Quaternion.identity;
					currentJetpack.Disable();
					if (firstJetPack)
					{
						firstJetPack = false;
						RevealDispenseJetPack();
					}
					else
					{
						animator.SetTrigger(dispenseHash);
					}
					NetSurrogate.IntVal = currentJetpack.ID;
					timeSinceLastJetpack = 0f;
					if (!InfiniteJetpacks)
					{
						jetpacksLeft--;
					}
					if (LobbyManager.instance.CurrentGameController != null)
					{
						LobbyManager.instance.CurrentGameController.SpawnNetSurrogate(currentJetpack.ID);
					}
				}
			}
			else if (NetSurrogate.IntVal != lastNetIDofJetpack)
			{
				lastNetIDofJetpack = NetSurrogate.IntVal;
				currentJetpack = Object.Instantiate(JetpackPrefab, JetpackSpawnPoint.transform.position, base.transform.rotation);
				currentJetpack.transform.parent = base.transform;
				currentJetpack.IsSaveable = false;
				currentJetpack.Place(0);
				currentJetpack.transform.localRotation = Quaternion.identity;
				timeSinceLastJetpack = 0f;
				currentJetpack.Disable();
				if (firstJetPack)
				{
					firstJetPack = false;
					RevealDispenseJetPack();
				}
				else
				{
					animator.SetTrigger(dispenseHash);
				}
				if (!InfiniteJetpacks)
				{
					jetpacksLeft--;
				}
				currentJetpack.ID = NetSurrogate.IntVal;
			}
		}
		else
		{
			Debug.LogError("Net surrogate for jetpack dispenser is null!");
		}
	}

	protected override void ToPlaceMode(bool enableSelection)
	{
		base.ToPlaceMode(enableSelection);
		Object.Destroy(currentJetpack.gameObject);
		FakeJetpack.enabled = true;
	}

	protected override void Activate()
	{
		base.Activate();
		jetpacksLeft = JetpacksPerRound;
		timeSinceLastJetpack = JetpackCooldown;
		FakeJetpack.enabled = true;
		firstJetPack = true;
	}

	public void RevealDispenseJetPack()
	{
		currentJetpack.Enable();
		currentJetpack.ToPlayMode();
		currentJetpack.Active = true;
		FakeJetpack.enabled = false;
	}

	public override void Reset()
	{
		base.Reset();
		animator.SetTrigger(resetHash);
		animator.ResetTrigger(dispenseHash);
		if (currentJetpack != null)
		{
			currentJetpack.DestroySelf(destroyChildren: false, useSmoke: false);
		}
	}

	public override void DestroySelf(bool destroyChildren = false, bool useSmoke = true, bool sendNetworkSignal = true)
	{
		if (currentJetpack != null)
		{
			currentJetpack.DestroySelf(destroyChildren, useSmoke, sendNetworkSignal);
		}
		else if (useSmoke && FakeJetpack.enabled && JetpackPrefab.explosionSmokeDebris != null)
		{
			Object.Instantiate(JetpackPrefab.explosionSmokeDebris, JetpackSpawnPoint.transform.position, base.transform.rotation).transform.localScale = base.transform.localScale;
		}
		base.DestroySelf(destroyChildren, useSmoke, sendNetworkSignal);
	}
}
