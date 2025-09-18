using UnityEngine;

public class ColliderModeControl : MonoBehaviour
{
	public bool PlacementPhase;

	public bool PlacedPhase;

	public bool RunPhase;

	public bool AlwaysOnInVersus;

	protected ColliderModeEnum currentPhase = ColliderModeEnum.PlacedPhase;

	public ColliderModeEnum CurrentPhase => currentPhase;

	protected virtual void Awake()
	{
		SwitchToMode(ColliderModeEnum.PlacementPhase, forceUpdate: true);
	}

	public virtual void SwitchToMode(ColliderModeEnum newPhase, bool forceUpdate = false)
	{
		if (!forceUpdate && currentPhase == newPhase)
		{
			return;
		}
		if (AlwaysOnInVersus)
		{
			GameState.GameMode gameMode = GameSettings.GetInstance().GameMode;
			if (gameMode != GameState.GameMode.FREEPLAY && (uint)(gameMode - 1) <= 2u)
			{
				Switch(OnOff: true);
				return;
			}
		}
		switch (newPhase)
		{
		case ColliderModeEnum.PlacementPhase:
			Switch(PlacementPhase);
			break;
		case ColliderModeEnum.PlacedPhase:
			Switch(PlacedPhase);
			break;
		case ColliderModeEnum.RunPhase:
			Switch(RunPhase);
			break;
		case ColliderModeEnum.NoColliders:
			Switch(OnOff: false);
			break;
		}
		currentPhase = newPhase;
	}

	public void Switch(bool OnOff)
	{
		if (OnOff)
		{
			Enable();
		}
		else
		{
			Disable();
		}
	}

	public virtual void Enable()
	{
		Collider2D[] components = GetComponents<Collider2D>();
		foreach (Collider2D collider2D in components)
		{
			if (!collider2D.enabled)
			{
				collider2D.enabled = true;
			}
		}
	}

	public virtual void Disable()
	{
		Collider2D[] components = GetComponents<Collider2D>();
		foreach (Collider2D collider2D in components)
		{
			if (collider2D.enabled)
			{
				collider2D.enabled = false;
			}
		}
	}
}
