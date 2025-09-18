using UnityEngine;

public class TabletSimpleScroll : MonoBehaviour
{
	public RectTransform scrollContents;

	public RectTransform scrollContainer;

	public TabletButton scrollPlusButton;

	public TabletButton scrollMinusButton;

	public float scrollAmount = 300f;

	public float targetPosition;

	public AnimationCurve ScrollSpeed = new AnimationCurve();

	private void Start()
	{
	}

	public void ResetScrolling()
	{
		scrollContents.anchoredPosition = Vector2.zero;
	}

	public void OnClickScrollPlus(PickCursor pickCursor)
	{
		targetPosition -= scrollAmount;
		ClampMinTargetPosition();
	}

	public void OnClickScrollMinus(PickCursor pickCursor)
	{
		targetPosition += scrollAmount;
		ClampMaxTargetPosition();
	}

	public void ApplyScrolling(float yScroll)
	{
		targetPosition -= yScroll * scrollAmount * 10f;
		if (yScroll <= 0f)
		{
			ClampMinTargetPosition();
		}
		else
		{
			ClampMaxTargetPosition();
		}
	}

	private void ClampMinTargetPosition()
	{
		if (targetPosition < 0f)
		{
			targetPosition = 0f;
		}
	}

	private void ClampMaxTargetPosition()
	{
		float height = scrollContents.rect.height;
		float height2 = scrollContainer.rect.height;
		float num = Mathf.Max(0f, height - height2);
		if (targetPosition > num)
		{
			targetPosition = num;
		}
	}

	private void Update()
	{
		if (scrollContainer.gameObject.activeSelf)
		{
			float height = scrollContents.rect.height;
			float height2 = scrollContainer.rect.height;
			if (height <= height2)
			{
				scrollMinusButton.SetDisabled(disabled: true);
				scrollPlusButton.SetDisabled(disabled: true);
				return;
			}
			float num = Mathf.Max(0f, height - height2);
			scrollContents.anchoredPosition = new Vector2(0f, Mathf.MoveTowards(scrollContents.anchoredPosition.y, targetPosition, ScrollSpeed.Evaluate(Mathf.Abs(scrollContents.anchoredPosition.y - targetPosition) * Time.deltaTime)));
			Vector2 anchoredPosition = scrollContents.anchoredPosition;
			if (anchoredPosition.y < 1f)
			{
				anchoredPosition.y = 0f;
			}
			if (anchoredPosition.y > num + 1f)
			{
				anchoredPosition.y = num;
			}
			scrollContents.anchoredPosition = anchoredPosition;
			scrollPlusButton.SetDisabled(targetPosition == 0f);
			scrollMinusButton.SetDisabled(targetPosition >= num - 1f);
		}
		else
		{
			scrollMinusButton.SetDisabled(disabled: true);
			scrollPlusButton.SetDisabled(disabled: true);
		}
	}
}
