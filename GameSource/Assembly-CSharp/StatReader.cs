using UnityEngine;
using UnityEngine.UI;

public class StatReader : MonoBehaviour
{
	public Text TextField;

	protected virtual void Start()
	{
		TextField = GetComponent<Text>();
	}

	private void Update()
	{
	}

	public void Reload()
	{
		TextField.text = getValue();
	}

	protected virtual string getValue()
	{
		return "N/A";
	}
}
