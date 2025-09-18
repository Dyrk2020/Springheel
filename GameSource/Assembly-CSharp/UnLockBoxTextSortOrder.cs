using UnityEngine;

public class UnLockBoxTextSortOrder : MonoBehaviour
{
	public Canvas UnlockBoxTextAndImage;

	public void MoveToFrontSortOrder()
	{
		UnlockBoxTextAndImage.sortingOrder = 15;
	}

	public void MoveToEffectSortOrder()
	{
		UnlockBoxTextAndImage.sortingOrder = 0;
		UnlockBoxTextAndImage.sortingLayerName = "Effects";
	}

	public void MoveToUISortOrder()
	{
		UnlockBoxTextAndImage.sortingOrder = 15;
		UnlockBoxTextAndImage.sortingLayerName = "UI 1";
	}
}
