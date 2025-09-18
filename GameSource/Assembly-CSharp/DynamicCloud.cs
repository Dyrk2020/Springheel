using UnityEngine;

public class DynamicCloud : MonoBehaviour
{
	private CloudScroller mCloudScroller;

	private DecorativeItemBlock mDecorativeItemBlock;

	private void Awake()
	{
		mDecorativeItemBlock = GetComponent<DecorativeItemBlock>();
		if (mDecorativeItemBlock != null)
		{
			mDecorativeItemBlock.OnPlaced += AttachToCloudHolder;
		}
	}

	private void OnDestroy()
	{
		if (mDecorativeItemBlock != null)
		{
			mDecorativeItemBlock.OnPlaced -= AttachToCloudHolder;
		}
		if (mCloudScroller != null)
		{
			mCloudScroller.UnregisterCloud(base.transform);
		}
	}

	private void AttachToCloudHolder()
	{
		if (mCloudScroller == null)
		{
			mCloudScroller = Object.FindObjectOfType<CloudScroller>();
		}
		if (mCloudScroller != null && mCloudScroller.cloudHolder != null)
		{
			base.transform.SetParent(mCloudScroller.cloudHolder.transform, worldPositionStays: true);
			mCloudScroller.RegisterCloud(base.transform);
		}
	}
}
