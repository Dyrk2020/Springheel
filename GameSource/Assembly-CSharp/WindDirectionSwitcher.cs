using GameEvent;
using UnityEngine;

public class WindDirectionSwitcher : MonoBehaviour, IGameEventListener
{
	public Vector3[] windDirections = new Vector3[3]
	{
		new Vector3(-1f, 0f, 0f),
		new Vector3(1f, 0f, 0f),
		new Vector3(0f, 0f, 0f)
	};

	public Transform[] goalFlags = new Transform[3];

	public WindArea[] windAreas;

	public int phaseCounter;

	private void OnDestroy()
	{
		ChangeListener(adding: false);
	}

	private void Start()
	{
		ChangeListener(adding: true);
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
			if (obj.Phase == GameControl.GamePhase.PLAY)
			{
				OnSequenceChange();
			}
			if (obj.Phase == GameControl.GamePhase.PLACE)
			{
				UpdateGoalFlags();
			}
			if (obj.Phase == GameControl.GamePhase.SUDDENDEATH)
			{
				OnSequenceChange();
			}
		}
	}

	private void OnSequenceChange()
	{
		Debug.LogWarning("WIND: Moving to wind sequence number " + (phaseCounter + 1) + " (" + windDirections[phaseCounter].ToString() + ")");
		WindArea[] array = windAreas;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].blowDirection = windDirections[phaseCounter];
		}
		UpdateGoalFlags();
		phaseCounter = (phaseCounter + 1) % windDirections.Length;
	}

	private void UpdateGoalFlags()
	{
		for (int i = 0; i < goalFlags.Length; i++)
		{
			if (goalFlags[i] != null)
			{
				goalFlags[i].gameObject.SetActive(i == phaseCounter);
			}
		}
	}
}
