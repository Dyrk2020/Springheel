using UnityEngine;

public class ToxicWaste : ActiveBlock
{
	private static readonly int InGameplay = Animator.StringToHash("inGameplay");

	private Animator _animator;

	private Animator[] _allAnimators;

	private ToxicPoolManager _toxicPoolManager;

	protected override void Awake()
	{
		base.Awake();
		AkSoundEngine.PostEvent("SFX_Level_ToxicTower", base.gameObject);
		_toxicPoolManager = GetComponent<ToxicPoolManager>();
		_animator = GetComponent<Animator>();
		_allAnimators = GetComponentsInChildren<Animator>(includeInactive: true);
	}

	protected override void Activate()
	{
		base.Activate();
		_animator.SetBool(InGameplay, value: true);
	}

	public override void Reset()
	{
		base.Reset();
		_animator.SetBool(InGameplay, value: false);
		_toxicPoolManager.DoReset();
	}

	public override void Pause()
	{
		base.Pause();
		_animator.speed = 0f;
		Animator[] allAnimators = _allAnimators;
		for (int i = 0; i < allAnimators.Length; i++)
		{
			allAnimators[i].speed = 0f;
		}
	}

	public override void Unpause()
	{
		base.Unpause();
		_animator.speed = 1f;
		Animator[] allAnimators = _allAnimators;
		for (int i = 0; i < allAnimators.Length; i++)
		{
			allAnimators[i].speed = 1f;
		}
	}
}
