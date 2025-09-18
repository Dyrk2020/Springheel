using UnityEngine;
using UnityEngine.UI;

public class BlankLevelTextHider : MonoBehaviour
{
	public Text textElement;

	public GameObject textBackground;

	private void Update()
	{
		bool active = GameSettings.GetInstance().GameMode != GameState.GameMode.FREEPLAY;
		textElement.enabled = active;
		if (textBackground != null)
		{
			textBackground.SetActive(active);
		}
	}
}
