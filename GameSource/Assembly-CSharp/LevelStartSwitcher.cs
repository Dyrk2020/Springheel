using GameEvent;
using UnityEngine;

public class LevelStartSwitcher : MonoBehaviour, IGameEventListener
{
	public int phaseCounter;

	public LevelStartInfo[] levelStarts = new LevelStartInfo[0];

	private Level currentLevel;

	private void OnDestroy()
	{
		ChangeListener(adding: false);
	}

	private void Start()
	{
		ChangeListener(adding: true);
		currentLevel = GetComponent<Level>();
		if (levelStarts.Length != 0)
		{
			levelStarts[0].ApplyToLevel(currentLevel);
		}
	}

	public void ChangeListener(bool adding)
	{
		GameEventManager.ChangeListener<StartPhaseEvent>(this, adding);
	}

	public void handleEvent(global::GameEvent.GameEvent e)
	{
		if (e.GetType() == typeof(StartPhaseEvent))
		{
			StartPhaseEvent obj = e as StartPhaseEvent;
			if (obj.Phase == GameControl.GamePhase.START && GameSettings.GetInstance().GameMode == GameState.GameMode.CHALLENGE)
			{
				OnSequenceChange();
			}
			_ = obj.Phase;
			_ = 2;
			if (obj.Phase == GameControl.GamePhase.PLACE)
			{
				OnSequenceChange();
			}
			if (obj.Phase == GameControl.GamePhase.SUDDENDEATH)
			{
				OnSequenceChange();
			}
		}
	}

	private void OnSequenceChange()
	{
		Debug.LogWarning("MULTIPLE STARTS: Changing to Start Position " + (phaseCounter + 1));
		for (int i = 0; i < levelStarts.Length; i++)
		{
			if (i == phaseCounter)
			{
				levelStarts[i].ApplyToLevel(currentLevel);
			}
			else
			{
				levelStarts[i].Hide();
			}
		}
		phaseCounter = (phaseCounter + 1) % levelStarts.Length;
	}
}
