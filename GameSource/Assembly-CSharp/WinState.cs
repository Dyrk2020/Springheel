using UnityEngine;
using UnityEngine.UI;

public class WinState : MonoBehaviour
{
	private Text text;

	public float winValue;

	public bool beat;

	public virtual void Start()
	{
		text = GetComponentInChildren<Text>();
		text.color = GameSettings.GetInstance().unbeatColor;
	}

	private void Update()
	{
	}

	public void Beat()
	{
		beat = true;
		text.color = GameSettings.GetInstance().beatColor;
	}

	public void Reset()
	{
		beat = false;
		text.color = GameSettings.GetInstance().unbeatColor;
	}
}
