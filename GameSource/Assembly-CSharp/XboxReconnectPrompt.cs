using UnityEngine;
using UnityEngine.UI;

public class XboxReconnectPrompt : MonoBehaviour
{
	public Text Message;

	public MultiControllerButton Button;

	protected Image Background;

	private void Awake()
	{
		Background = GetComponentInChildren<Image>();
	}

	private void Start()
	{
		Button.forceControllerType = true;
		Button.forceControllerType = false;
	}

	public void Show()
	{
		Message.enabled = true;
		Button.Hidden = false;
		Background.enabled = true;
	}

	public void Hide()
	{
		Message.enabled = false;
		Button.Hidden = true;
		Background.enabled = false;
	}
}
