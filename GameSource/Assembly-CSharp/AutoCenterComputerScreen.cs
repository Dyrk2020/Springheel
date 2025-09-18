using UnityEngine;

public class AutoCenterComputerScreen : MonoBehaviour
{
	private InventoryBook book;

	public float offset;

	public bool ApplyAspectRatioScaling;

	public float MaxAspectRatio = 1.7777778f;

	public Vector2 ReferenceScale = new Vector2(2.238f, 1.28f);

	private void Start()
	{
		InventoryPage component = GetComponent<InventoryPage>();
		book = component.inventoryBook;
	}

	private void Update()
	{
		if (book != null && book.UiCamera != null)
		{
			Vector3 position = base.transform.position;
			position.x = book.UiCamera.transform.position.x + offset;
			base.transform.position = position;
			ApplyAspectScaling();
		}
	}

	private void ApplyAspectScaling()
	{
		if (ApplyAspectRatioScaling)
		{
			float num = Mathf.Min(book.UiCamera.aspect, MaxAspectRatio);
			float x = ReferenceScale.x / MaxAspectRatio * num;
			base.transform.localScale = new Vector3(x, ReferenceScale.y, 1f);
		}
	}
}
