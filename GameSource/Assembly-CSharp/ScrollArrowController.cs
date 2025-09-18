using UnityEngine;

public class ScrollArrowController : MonoBehaviour
{
	public RectTransform scrollContents;

	public RectTransform scrollContainer;

	public HoldableButton scrollPlusButton;

	public HoldableButton scrollMinusButton;

	public float autoScroll;

	private void Start()
	{
		scrollMinusButton.gameObject.SetActive(value: false);
		scrollPlusButton.gameObject.SetActive(value: false);
	}

	public void ResetScrolling()
	{
		scrollContents.anchoredPosition = Vector2.zero;
	}

	public void OnClickScrollbarPlus()
	{
		Vector2 anchoredPosition = scrollContents.anchoredPosition;
		anchoredPosition.y -= 100f;
		if (anchoredPosition.y < 0f)
		{
			anchoredPosition.y = 0f;
		}
		scrollContents.anchoredPosition = anchoredPosition;
	}

	public void OnClickScrollbarMinus()
	{
		float height = scrollContents.rect.height;
		float height2 = scrollContainer.rect.height;
		float num = Mathf.Max(0f, height - height2);
		Vector2 anchoredPosition = scrollContents.anchoredPosition;
		anchoredPosition.y += 100f;
		if (anchoredPosition.y > num)
		{
			anchoredPosition.y = num;
		}
		scrollContents.anchoredPosition = anchoredPosition;
	}

	private void Update()
	{
		if (scrollContainer.gameObject.activeSelf)
		{
			float height = scrollContents.rect.height;
			float height2 = scrollContainer.rect.height;
			if (height <= height2)
			{
				scrollMinusButton.gameObject.SetActive(value: false);
				scrollPlusButton.gameObject.SetActive(value: false);
				return;
			}
			float num = Mathf.Max(0f, height - height2);
			Vector2 anchoredPosition = scrollContents.anchoredPosition;
			if (scrollPlusButton.Held)
			{
				float num2 = (scrollPlusButton.HeldWithSprint ? 1.5f : 1f);
				anchoredPosition.y -= 1000f * num2 * Time.deltaTime;
			}
			if (scrollMinusButton.Held)
			{
				float num3 = (scrollMinusButton.HeldWithSprint ? 1.5f : 1f);
				anchoredPosition.y += 1000f * num3 * Time.deltaTime;
			}
			anchoredPosition.y += 1000f * autoScroll * Time.deltaTime;
			anchoredPosition.y = Mathf.Clamp(anchoredPosition.y, 0f, num);
			scrollContents.anchoredPosition = anchoredPosition;
			scrollPlusButton.gameObject.SetActive(anchoredPosition.y > 0.01f);
			scrollMinusButton.gameObject.SetActive(anchoredPosition.y < num - 0.01f);
		}
		else
		{
			scrollPlusButton.gameObject.SetActive(value: false);
			scrollMinusButton.gameObject.SetActive(value: false);
		}
	}

	public bool OnPickCursorScrollPlus(PickCursor pickCursor)
	{
		if (PickableButton.IsPointInRectTransform(pickCursor.cursorPoint.position, scrollContainer))
		{
			OnClickScrollbarPlus();
			return true;
		}
		return false;
	}

	public bool OnPickCursorScrollMinus(PickCursor pickCursor)
	{
		if (PickableButton.IsPointInRectTransform(pickCursor.cursorPoint.position, scrollContainer))
		{
			OnClickScrollbarMinus();
			return true;
		}
		return false;
	}
}
