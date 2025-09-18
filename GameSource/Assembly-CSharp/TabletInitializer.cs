using UnityEngine;

public class TabletInitializer : MonoBehaviour
{
	public InventoryBook book;

	public Object tabletScreenPrefab;

	private void Awake()
	{
		InventoryPage componentInChildren = book.gameObject.AddPrefabAsChild(tabletScreenPrefab).GetComponentInChildren<InventoryPage>();
		book.TabletPage = componentInChildren;
		componentInChildren.inventoryBook = book;
	}
}
