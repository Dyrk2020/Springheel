using I2.Loc;
using UnityEngine;
using UnityEngine.UI;

public class LobbyWaitingText : MonoBehaviour
{
	public bool isHost;

	private bool firstUpdate = true;

	private string externalIP;

	private bool textReplaced;

	private Text ipLabel;

	private void Awake()
	{
		ipLabel = GetComponent<Text>();
	}

	private void OnEnable()
	{
		firstUpdate = true;
		textReplaced = false;
	}

	private void Update()
	{
		if (firstUpdate)
		{
			firstUpdate = false;
			if (!isHost)
			{
				ipLabel.text = LocalizationManager.GetTranslation("Title/WaitForPlayerConfirm");
			}
			else
			{
				ipLabel.text = LocalizationManager.GetTranslation("Title/GatheringNetwork");
			}
		}
		if (isHost && !textReplaced)
		{
			externalIP = Matchmaker.Instance.myExternalIP;
			if (!externalIP.Contains("UNASSIGNED"))
			{
				ipLabel.text = externalIP.ToString();
				textReplaced = true;
			}
		}
	}
}
