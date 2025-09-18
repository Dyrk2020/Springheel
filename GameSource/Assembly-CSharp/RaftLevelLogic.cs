using GameEvent;
using UnityEngine;

public class RaftLevelLogic : MonoBehaviour, IGameEventListener
{
	private static readonly int HighTide = Animator.StringToHash("highTide");

	public KrakenTrigger krakenTrigger;

	private bool _isFirstPlayTime = true;

	private Animator _animator;

	private float _lastEndPlayTime;

	private void Awake()
	{
		_animator = GetComponent<Animator>();
	}

	private void OnEnable()
	{
		GameEventManager.ChangeListener<EndPhaseEvent>(this, adding: true);
	}

	private void OnDisable()
	{
		GameEventManager.ChangeListener<EndPhaseEvent>(this, adding: false);
	}

	public void handleEvent(global::GameEvent.GameEvent e)
	{
		if (e is EndPhaseEvent { Phase: GameControl.GamePhase.PLAY } && _lastEndPlayTime + 1f < Time.time)
		{
			bool flag = _animator.GetBool(HighTide);
			_animator.SetBool(HighTide, !flag);
			krakenTrigger.SetActive(!flag);
			if (flag)
			{
				RaftDayNightCycle.instance.DayTransition();
			}
			else
			{
				RaftDayNightCycle.instance.NightTransition();
			}
			_lastEndPlayTime = Time.time;
		}
	}
}
