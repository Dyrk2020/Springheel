using GameEvent;
using UnityEngine;

public class AnimationElement : MonoBehaviour, IGameEventListener
{
	public string animName = "ElectricityLoop";

	private Animator animator;

	private SpriteRenderer sprite;

	private void Awake()
	{
		sprite = GetComponent<SpriteRenderer>();
		animator = GetComponent<Animator>();
	}

	private void OnEnable()
	{
		ChangeListener(adding: true);
	}

	private void OnDisable()
	{
		ChangeListener(adding: false);
	}

	public virtual void ChangeListener(bool adding)
	{
		GameEventManager.ChangeListener<StartPhaseEvent>(this, adding);
		GameEventManager.ChangeListener<PauseEvent>(this, adding);
		GameEventManager.ChangeListener<EndPhaseEvent>(this, adding);
		GameEventManager.ChangeListener<LevelResetEvent>(this, adding);
	}

	public void handleEvent(global::GameEvent.GameEvent e)
	{
		if (e is StartPhaseEvent startPhaseEvent)
		{
			if (startPhaseEvent.Phase == GameControl.GamePhase.PLAY)
			{
				StartPlaying();
			}
			if (startPhaseEvent.Phase == GameControl.GamePhase.PLACE)
			{
				StopPlaying();
			}
			if (startPhaseEvent.Phase == GameControl.GamePhase.SUDDENDEATH)
			{
				StartPlaying();
			}
		}
		if (e is PauseEvent pauseEvent)
		{
			animator.speed = ((!pauseEvent.Paused) ? 1 : 0);
		}
	}

	private void StartPlaying()
	{
		sprite.enabled = true;
		animator.enabled = true;
		animator.Play(animName, 0, 0f);
	}

	private void StopPlaying()
	{
		sprite.enabled = false;
		animator.enabled = false;
	}
}
