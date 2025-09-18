using System;
using System.Collections.Generic;
using System.Linq;
using GameEvent;
using UnityEngine;

public class CrumblingBlockController : MonoBehaviour, IGameEventListener
{
	public List<CrumblingBlock> levelBlocks;

	private void Start()
	{
		ChangeListener(adding: true);
		if (levelBlocks == null || levelBlocks.Count == 0)
		{
			Debug.LogError("No crumbling blocks defined. Adding all crumbling blocks in level automatically.");
			AddAllCrumblingBlocksInScene();
		}
		foreach (CrumblingBlock levelBlock in levelBlocks)
		{
			levelBlock.AllowReset = true;
		}
	}

	private void OnDestroy()
	{
		ChangeListener(adding: false);
	}

	public void ChangeListener(bool adding)
	{
		GameEventManager.ChangeListener<StartPhaseEvent>(this, adding);
		GameEventManager.ChangeListener<LevelResetEvent>(this, adding);
		GameEventManager.ChangeListener<FreePlayCharacterRespawnEvent>(this, adding);
	}

	public void handleEvent(global::GameEvent.GameEvent e)
	{
		Type type = e.GetType();
		if (type == typeof(StartPhaseEvent) && (e as StartPhaseEvent).Phase == GameControl.GamePhase.PLACE)
		{
			GameState.GameMode gameMode = GameSettings.GetInstance().GameMode;
			if ((uint)(gameMode - 1) > 1u)
			{
				ResetCrumblingBlocks();
			}
		}
		if (type == typeof(LevelResetEvent) || type == typeof(FreePlayCharacterRespawnEvent))
		{
			ResetCrumblingBlocks();
		}
	}

	private void ResetCrumblingBlocks()
	{
		foreach (CrumblingBlock levelBlock in levelBlocks)
		{
			if (levelBlock != null)
			{
				levelBlock.ResetDamage();
			}
		}
	}

	public void AddAllCrumblingBlocksInScene()
	{
		levelBlocks = UnityEngine.Object.FindObjectsOfType<CrumblingBlock>().ToList();
	}
}
