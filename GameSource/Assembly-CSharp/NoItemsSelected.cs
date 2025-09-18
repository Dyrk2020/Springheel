using System.Collections;
using GameEvent;
using UnityEngine;

public class NoItemsSelected : UIGraphic, IGameEventListener
{
	public float showTime = 5f;

	private void Start()
	{
		ChangeListener(adding: true);
		Hide(forceQuickHide: true);
	}

	public void ChangeListener(bool adding)
	{
		GameEventManager.ChangeListener<SpecialUIEvent>(this, adding);
	}

	private void OnDestroy()
	{
		ChangeListener(adding: false);
	}

	public virtual void handleEvent(global::GameEvent.GameEvent e)
	{
		if (e.GetType() == typeof(SpecialUIEvent) && (e as SpecialUIEvent).SpecialUIType == SpecialUIEvent.SpecialUI.NOITEMSELECTED)
		{
			StartCoroutine(showInfo(showTime));
		}
	}

	private IEnumerator showInfo(float time)
	{
		if (!base.Visible)
		{
			Show();
			float timer = 0f;
			do
			{
				timer += Time.unscaledDeltaTime;
				yield return null;
			}
			while (timer < time && GameSettings.GetInstance().AvailableBlocks == 0);
			Hide();
		}
	}
}
