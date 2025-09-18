using GameEvent;
using UnityEngine;
using UnityEngine.UI;

public class PickableOptionSlider : PickableButton, IGameEventListener
{
	public enum OptionSliderJobs
	{
		Music,
		SFX
	}

	public OptionSliderJobs job;

	public Transform top;

	public Transform bottom;

	public GameObject slider;

	public bool held;

	public PickCursor currentCursor;

	protected override void Awake()
	{
		base.Awake();
		Outline outline = slider.AddComponent<Outline>();
		outline.effectDistance = new Vector2(outlineHighlightSizer, outlineHighlightSizer);
		outline.effectColor = new Color(0f, 0f, 0f, 0f);
		outlines.Add(outline);
	}

	protected override void Start()
	{
		base.Start();
		UpdateSoundSliderPositions();
	}

	private void UpdateSoundSliderPositions()
	{
		if (ControllerMonitor.Instance.IsMainControllerSet)
		{
			switch (job)
			{
			case OptionSliderJobs.Music:
				SetPosition(GameSettings.Music);
				SetAssociatedValue(getValue());
				break;
			case OptionSliderJobs.SFX:
				SetPosition(GameSettings.Sound);
				SetAssociatedValue(getValue());
				break;
			}
		}
	}

	public override void ChangeListener(bool adding)
	{
		base.ChangeListener(adding);
		GameEventManager.ChangeListener<InventoryPageDisplayEvent>(this, adding);
	}

	public override void handleEvent(global::GameEvent.GameEvent e)
	{
		base.handleEvent(e);
		if (e.GetType() == typeof(InventoryPageDisplayEvent) && (e as InventoryPageDisplayEvent).pageNumber == pageNumber)
		{
			UpdateSoundSliderPositions();
		}
	}

	private void SetPosition(float inputValue)
	{
		inputValue = Mathf.Clamp01(inputValue);
		Vector3 position = Vector3.Lerp(bottom.position, top.position, inputValue);
		slider.transform.position = position;
	}

	private float getValue()
	{
		return (slider.transform.position.x - bottom.position.x) / (top.transform.position.x - bottom.transform.position.x);
	}

	public override void OnAccept(PickCursor pickCursor)
	{
		base.OnAccept(pickCursor);
		held = true;
		currentCursor = pickCursor;
	}

	protected override void Update()
	{
		base.Update();
		if (!Visible || !initialized)
		{
			return;
		}
		if (job != OptionSliderJobs.Music)
		{
			_ = 1;
		}
		if (held && !currentCursor.Held)
		{
			held = false;
			if (ClickSoundEvent != "")
			{
				AkSoundEngine.PostEvent(ClickSoundEvent, base.gameObject);
			}
			if (ControllerMonitor.Instance.IsMainControllerSet)
			{
				StatTracker.Instance.SaveGameForAllUsers();
			}
		}
		if (held)
		{
			slider.transform.position = currentCursor.cursorPoint.position;
			float x = Mathf.Clamp(slider.transform.position.x, bottom.position.x, top.position.x);
			slider.transform.position = new Vector3(x, bottom.transform.position.y, 0f);
			SetAssociatedValue(getValue());
		}
	}

	public void SetAssociatedValue(float value)
	{
		SaveFileData saveFileDataForMainUser = StatTracker.Instance.GetSaveFileDataForMainUser();
		switch (job)
		{
		case OptionSliderJobs.Music:
			GameSettings.Music = value;
			if (!WwiseSuspender.Muted)
			{
				AkSoundEngine.SetRTPCValue("MUS_volume", value * 100f);
			}
			saveFileDataForMainUser.MusicVolume = value;
			break;
		case OptionSliderJobs.SFX:
			GameSettings.Sound = value;
			if (!WwiseSuspender.Muted)
			{
				AkSoundEngine.SetRTPCValue("SFX_volume", value * 100f);
			}
			saveFileDataForMainUser.SoundVolume = value;
			break;
		}
	}
}
