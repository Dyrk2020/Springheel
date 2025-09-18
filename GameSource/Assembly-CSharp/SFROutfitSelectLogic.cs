using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class SFROutfitSelectLogic : MonoBehaviour
{
	public static SFROutfitSelectLogic Instance;

	public Text titleName;

	public SFROutfitToggle outfitTogglePrefab;

	public Transform outfitToggleContainer;

	private UnityAction onFinish;

	private void Awake()
	{
		Instance = this;
		base.gameObject.SetActive(value: false);
	}

	public void Initialize(Character.Animals animal, StatReaderCharacter statReader)
	{
		base.gameObject.SetActive(value: true);
		titleName.text = "Select outfits for " + animal;
		SaveFileData saveFileDataForMainUser = StatTracker.Instance.GetSaveFileDataForMainUser();
		StatCountArray stat = saveFileDataForMainUser.GetStat<StatCountArray>("OutfitsUnlocked");
		int num = stat.values[(int)animal];
		outfitToggleContainer.DestroyAllChildren();
		List<SFROutfitToggle> toggleList = new List<SFROutfitToggle>();
		for (int i = 1; i < 32; i++)
		{
			int num2 = i - 1;
			UnLockInfo outfitUnlock = UnlockInfoLibrary.Instance.GetOutfitUnlock(animal, i);
			if (outfitUnlock != null)
			{
				SFROutfitToggle sFROutfitToggle = outfitToggleContainer.gameObject.AddPrefabAsChild<SFROutfitToggle>(outfitTogglePrefab.gameObject);
				bool isOn = (num >> num2) % 2 != 0;
				sFROutfitToggle.Initialize(outfitUnlock.UpperString, isOn);
				toggleList.Add(sFROutfitToggle);
			}
		}
		onFinish = delegate
		{
			int num3 = 0;
			for (int j = 0; j < toggleList.Count; j++)
			{
				num3 += (toggleList[j].toggle.isOn ? 1 : 0) << j;
			}
			stat.Set((int)animal, num3);
			statReader.Reload();
		};
	}

	public void OnClickOK()
	{
		onFinish();
		onFinish = null;
		base.gameObject.SetActive(value: false);
	}
}
