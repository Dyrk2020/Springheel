using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FeaturedBatchEntry : MonoBehaviour
{
	public Image bgImage;

	public Text label;

	public Text dirtyMarker;

	public Color SelectedBackgroundColor;

	public Color SelectedFontColor;

	public Color DeselectedBackgroundColor;

	public Color DeselectedFontColor;

	public string batchName;

	public string batchID = "NULL";

	public DateTime publishAfterUTC;

	public bool dirty;

	public List<string> codeList = new List<string>();

	public string code;

	public string levelName;

	private void Awake()
	{
		dirtyMarker.enabled = dirty;
	}

	public void InitializeBatch(string batchName, string batchID, AdminBatchManagementDialog adminDialog)
	{
		this.batchID = batchID;
		SetBatchName(batchName);
		GetComponent<GenericButton>().OnClickWithCursor.AddListener(adminDialog.OnClickBatch(this));
	}

	public void InitializeCode(string levelCode, string levelName, AdminBatchManagementDialog adminDialog)
	{
		code = levelCode;
		this.levelName = levelName;
		label.text = GameSparksQuery.GetFormattedSnapshotCode(code) + " - " + levelName;
		GetComponent<GenericButton>().OnClickWithCursor.AddListener(adminDialog.OnClickCode(this));
	}

	public void OnSelect()
	{
		label.color = SelectedFontColor;
		dirtyMarker.color = SelectedFontColor;
		bgImage.color = SelectedBackgroundColor;
	}

	public void OnDeselect()
	{
		label.color = DeselectedFontColor;
		dirtyMarker.color = DeselectedFontColor;
		bgImage.color = DeselectedBackgroundColor;
	}

	public void MarkDirty()
	{
		dirty = true;
		dirtyMarker.enabled = dirty;
	}

	public void ClearDirtyFlag()
	{
		dirty = false;
		dirtyMarker.enabled = dirty;
	}

	public void SetBatchName(string batchName)
	{
		this.batchName = batchName;
		label.text = batchName;
	}

	public void RefreshCodeList(Transform codeListContainer)
	{
		codeList.Clear();
		foreach (Transform item in codeListContainer)
		{
			FeaturedBatchEntry component = item.GetComponent<FeaturedBatchEntry>();
			if (component != null && !component.code.NullOrEmpty())
			{
				codeList.Add(component.code);
			}
		}
		MarkDirty();
	}
}
