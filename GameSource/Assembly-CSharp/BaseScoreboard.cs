using System.Collections;
using GameEvent;
using UnityEngine;

public class BaseScoreboard : MonoBehaviour, IGameEventListener
{
	public bool GameShowingScore;

	public bool Skip;

	public bool DrawingScore;

	public float SizeFactor = 1f;

	protected Animator animator;

	protected bool showing;

	protected bool actuallyVisible;

	protected bool actuallyHidden = true;

	public string showAudioEvent;

	public string hideAudioEvent;

	protected float scoreTimer;

	protected bool timed;

	protected const float ratio16by9 = 1.7777778f;

	protected Vector3 initialScale;

	protected float scoreboardDelay = 5f;

	public bool Showing => showing;

	protected void Awake()
	{
		animator = GetComponent<Animator>();
		Hide();
	}

	protected virtual void Start()
	{
		ChangeListener(adding: true);
		initialScale = base.transform.localScale;
	}

	protected void OnDestroy()
	{
		ChangeListener(adding: false);
	}

	public virtual void ChangeListener(bool adding)
	{
		GameEventManager.ChangeListener<SpecialUIEvent>(this, adding);
	}

	protected virtual void Update()
	{
	}

	public virtual void Show(bool GameShowing = true)
	{
		GameShowingScore = GameShowing;
		animator.SetBool("Faded", !GameShowingScore);
		actuallyHidden = false;
		if (!showing)
		{
			animator.SetBool("OnScreen", value: true);
			AkSoundEngine.PostEvent(showAudioEvent, base.gameObject);
			showing = true;
			if (Camera.main.aspect < 1.7777778f)
			{
				base.transform.localScale = Camera.main.aspect * initialScale / 1.7777778f;
			}
		}
	}

	public virtual void Hide(bool afterTally = false, bool allLocal = true)
	{
		GameShowingScore = false;
		actuallyVisible = false;
		if (showing)
		{
			animator.SetBool("OnScreen", value: false);
			AkSoundEngine.PostEvent(hideAudioEvent, base.gameObject);
			showing = false;
			if (allLocal)
			{
				GameEventManager.SendEvent(new ScoreboardEvent(show: false, afterTally));
			}
		}
	}

	public void ActuallyVisible()
	{
		actuallyVisible = true;
	}

	public void FinishedHiding()
	{
		actuallyHidden = true;
	}

	public virtual void handleEvent(global::GameEvent.GameEvent e)
	{
		if (e.GetType() == typeof(SpecialUIEvent) && (e as SpecialUIEvent).SpecialUIType == SpecialUIEvent.SpecialUI.SCOREBOARDDELAY)
		{
			StartCoroutine(DelayScoreboard());
		}
	}

	private IEnumerator DelayScoreboard()
	{
		animator.SetBool("DelayScoreboard", value: true);
		float delayTimer = 0f;
		do
		{
			delayTimer += Time.unscaledDeltaTime;
			yield return null;
		}
		while (delayTimer < scoreboardDelay);
		animator.SetBool("DelayScoreboard", value: false);
	}
}
