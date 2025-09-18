using UnityEngine;
using UnityEngine.UI;

public class SFROutfitToggle : MonoBehaviour
{
	public Toggle toggle;

	public Text toggleLabel;

	public void Initialize(string name, bool isOn)
	{
		toggle.isOn = isOn;
		toggleLabel.text = name;
	}
}
