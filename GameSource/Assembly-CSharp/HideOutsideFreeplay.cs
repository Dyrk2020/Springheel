using UnityEngine;

public class HideOutsideFreeplay : MonoBehaviour
{
	public GameObject[] objectsToHide;

	public void Awake()
	{
		if (GameSettings.GetInstance().GameMode != GameState.GameMode.FREEPLAY)
		{
			GameObject[] array = objectsToHide;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].SetActive(value: false);
			}
		}
	}
}
