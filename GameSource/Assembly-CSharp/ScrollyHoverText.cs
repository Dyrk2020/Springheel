using UnityEngine;
using UnityEngine.UI;

public class ScrollyHoverText : MonoBehaviour
{
	public Text scrollableText;

	public RectTransform scrollableElement;

	private RectTransform scrolledTransform;

	public RectTransform containerTransform;

	public PickableButton pickableButton;

	public bool alwaysScroll;

	public float scrollSpeed = 300f;

	private bool initialized;

	private bool scrolling;

	private float scrollTimer;

	public bool scrollOnce;

	private void Awake()
	{
		if (scrollableText != null)
		{
			scrolledTransform = scrollableText.rectTransform;
		}
		else
		{
			scrolledTransform = scrollableElement;
		}
	}

	private void Update()
	{
		if (!initialized)
		{
			initialized = true;
			RecenterText();
		}
		bool flag = scrollOnce || alwaysScroll || (pickableButton != null && pickableButton.HoveredCursors.Count > 0);
		bool flag2 = false;
		if (!scrolling && flag)
		{
			scrollTimer = 0f;
			scrolling = true;
			flag2 = true;
			RecenterText();
		}
		else if (scrolling && !flag)
		{
			scrollTimer = 0f;
			scrolling = false;
			RecenterText();
		}
		if (!scrolling)
		{
			return;
		}
		float width = scrolledTransform.rect.width;
		float width2 = containerTransform.rect.width;
		if (width > width2)
		{
			float x = scrolledTransform.pivot.x;
			float num = width2 * (1f - x) + width * x;
			float num2 = 0f - (width * (1f - x) + width2 * x);
			float num3 = num / scrollSpeed;
			float num4 = Mathf.Abs(num2) / scrollSpeed;
			float num5 = num3 + num4;
			if (flag2)
			{
				scrollTimer = num3;
			}
			Vector2 anchoredPosition = scrolledTransform.anchoredPosition;
			if (scrollTimer <= num3)
			{
				float t = scrollTimer / num3;
				anchoredPosition.x = Mathf.LerpUnclamped(num, 0f, t);
			}
			else
			{
				float t2 = (scrollTimer - num3) / num4;
				anchoredPosition.x = Mathf.LerpUnclamped(0f, num2, t2);
			}
			scrolledTransform.anchoredPosition = anchoredPosition;
			scrollTimer += Time.unscaledDeltaTime;
			if (!(scrollTimer > num5))
			{
				return;
			}
			scrollTimer -= num5;
			if (scrollOnce)
			{
				scrollOnce = false;
				scrollTimer = 0f;
				if (scrollableText != null)
				{
					scrollableText.text = "";
				}
				scrolling = false;
			}
		}
		else
		{
			RecenterText();
		}
	}

	private void RecenterText()
	{
		float width = scrolledTransform.rect.width;
		float width2 = containerTransform.rect.width;
		float y = scrolledTransform.anchoredPosition.y;
		if (width > width2)
		{
			scrolledTransform.anchoredPosition = new Vector2((width - width2) * 0.5f, y);
		}
		else
		{
			scrolledTransform.anchoredPosition = new Vector2(0f, y);
		}
	}

	public void ScrollMessageOnce(string message)
	{
		if (scrollableText != null)
		{
			scrollTimer = 0f;
			scrolling = true;
			scrollOnce = true;
			scrollableText.text = message;
		}
		else
		{
			Debug.LogError("Could not scroll message once: scrollableText is null");
		}
	}
}
