using UnityEngine;
using UnityEngine.UI;

public class TextChanger : MonoBehaviour
{
	public string nextString;

	public Text textBox;

	public void ChangeToNextString()
	{
		textBox.text = nextString;
	}
}
