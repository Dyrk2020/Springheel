using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class InventoryPage : MonoBehaviour
{
	public enum PageTypes
	{
		TitlePage,
		StartPage,
		RulePage,
		InventoryPage1,
		InventoryPage2,
		InventoryPage3,
		InventoryPage4,
		InventoryPage5,
		InventoryPage6,
		InventoryPage7,
		InventoryPage8,
		InventoryPage9,
		PlayOnlinePage,
		OnlineOptionsPage,
		OptionsPage,
		TwitchOptionsPage,
		ControlsPage,
		LanguagesPage,
		AnimalStats,
		ItemStats,
		LevelStats,
		Credits,
		PausePage,
		ShareableSnapshot,
		ComputerTerminal,
		TableOfContents,
		SecondComputerTerminal,
		nonePage,
		RulesPointTypes,
		CurrentSnapshotInfo,
		TabletInterface,
		TabletInterfaceMods
	}

	public InventoryPage useThisPrefabInstead;

	public List<IPickable> pickableOnPage = new List<IPickable>();

	public List<Text> textOnPage = new List<Text>();

	public List<Image> imagesOnPage = new List<Image>();

	public Canvas textCanvas;

	public Animator animator;

	public SpriteRenderer pagePaper;

	public SpriteRenderer pageEdge;

	private List<SpriteRenderer> spriteRenders = new List<SpriteRenderer>();

	private List<SortingGroup> sortingGroups = new List<SortingGroup>();

	public int pageNumber;

	public Text pageNumberText;

	public InventoryBook inventoryBook;

	public bool OnlineOnly;

	public bool FreeplayOnly;

	public bool BlankLevelOnly;

	public bool HideInChallengeMode;

	public bool Deactivated;

	public PageTypes pageType;

	public PageTypes ScreenBackButtonTarget;

	private void Awake()
	{
		animator = GetComponent<Animator>();
		if (animator == null)
		{
			animator = GetComponentInParent<Animator>();
			if (animator != null)
			{
				animator.transform.localPosition = new Vector3(0f, -50f, 0f);
			}
		}
		if (textCanvas == null)
		{
			textCanvas = GetComponentInChildren<Canvas>();
		}
		if (pageType != PageTypes.TabletInterface)
		{
			pickableOnPage.AddRange(GetComponentsInChildren<IPickable>());
			textOnPage.AddRange(GetComponentsInChildren<Text>());
			imagesOnPage.AddRange(GetComponentsInChildren<Image>());
			spriteRenders.AddRange(GetComponentsInChildren<SpriteRenderer>());
			sortingGroups.AddRange(GetComponentsInChildren<SortingGroup>());
		}
	}

	public void AddInstantiatedElements(Transform container)
	{
		IPickable[] componentsInChildren = container.gameObject.GetComponentsInChildren<IPickable>();
		pickableOnPage.AddRange(componentsInChildren);
		IPickable[] array = componentsInChildren;
		foreach (IPickable obj in array)
		{
			obj.PageNumber = pageNumber;
			obj.InventoryBook = inventoryBook;
			obj.Enable();
		}
		textOnPage.AddRange(container.GetComponentsInChildren<Text>());
		imagesOnPage.AddRange(container.GetComponentsInChildren<Image>());
		spriteRenders.AddRange(container.GetComponentsInChildren<SpriteRenderer>());
		sortingGroups.AddRange(GetComponentsInChildren<SortingGroup>());
	}

	public void SetPageNumber(int newPageNumber, int shownPageNumber, int totalPages = 0)
	{
		pageNumber = newPageNumber;
		foreach (IPickable item in pickableOnPage)
		{
			item.PageNumber = pageNumber;
		}
		string text = "";
		if (totalPages > 0)
		{
			text = " / " + totalPages;
		}
		if (pageNumberText != null)
		{
			pageNumberText.text = shownPageNumber + text;
		}
		else
		{
			Debug.Log("Warning! Page " + pageType.ToString() + " has no page number text element!");
		}
	}

	public void AddPickable(IPickable newPickable)
	{
		pickableOnPage.Add(newPickable);
		newPickable.PageNumber = pageNumber;
		newPickable.InventoryBook = inventoryBook;
	}

	public void RemovePickable(IPickable removePickable)
	{
		pickableOnPage.Remove(removePickable);
	}

	private void Update()
	{
	}

	public void showContent()
	{
		displayContent(show: true);
	}

	public void hideContent()
	{
		displayContent(show: false);
	}

	private void displayContent(bool show)
	{
		foreach (Text item in textOnPage)
		{
			if (item != null)
			{
				item.enabled = show;
			}
		}
		foreach (Image item2 in imagesOnPage)
		{
			if (item2 != null)
			{
				item2.enabled = show;
			}
		}
		foreach (SpriteRenderer spriteRender in spriteRenders)
		{
			if (spriteRender != null)
			{
				spriteRender.enabled = show;
			}
		}
		if (pagePaper != null)
		{
			pagePaper.enabled = true;
		}
		if (pageEdge != null)
		{
			pageEdge.enabled = true;
		}
		foreach (IPickable item3 in pickableOnPage)
		{
			MonoBehaviour monoBehaviour = item3 as MonoBehaviour;
			if (item3 != null && monoBehaviour != null)
			{
				if (show)
				{
					item3.Enable();
				}
				else
				{
					item3.Disable();
				}
			}
		}
	}

	private void hidePage()
	{
		pagePaper.enabled = false;
		pageEdge.enabled = false;
	}

	private void showPage()
	{
		pagePaper.enabled = true;
		pageEdge.enabled = true;
	}

	public void setPageState(int state)
	{
		animator.SetInteger("PageState", state);
	}

	public void setPageLayer(int num)
	{
		textCanvas.sortingOrder = num;
		foreach (SpriteRenderer spriteRender in spriteRenders)
		{
			spriteRender.sortingOrder = num;
		}
		foreach (SortingGroup sortingGroup in sortingGroups)
		{
			sortingGroup.sortingOrder = num;
		}
		foreach (IPickable item in pickableOnPage)
		{
			if (item.SpriteSortOrder != null)
			{
				item.SpriteSortOrder.setSortOrder(num);
			}
			item.SetTextCanvasOrder(num);
		}
		pagePaper.sortingOrder = num - 4;
		pageEdge.sortingOrder = num - 5;
	}

	public void setPageLayerFrontOfRing()
	{
		pagePaper.sortingLayerName = "Default";
	}

	public void setPageLayerBackOfRing()
	{
		pagePaper.sortingLayerName = "GraphPaper";
	}

	public void turnPageBack()
	{
	}

	public void turnPageForward()
	{
	}

	public void ReplaceSelfWithPrefab()
	{
		if ((bool)useThisPrefabInstead)
		{
			InventoryPage inventoryPage = Object.Instantiate(useThisPrefabInstead, base.transform.parent);
			inventoryPage.transform.SetSiblingIndex(base.transform.GetSiblingIndex());
			inventoryPage.transform.localPosition = base.transform.localPosition;
			inventoryPage.transform.localScale = base.transform.localScale;
			inventoryPage.transform.localRotation = base.transform.localRotation;
			inventoryPage.useThisPrefabInstead = useThisPrefabInstead;
			Object.Destroy(base.gameObject);
		}
	}
}
