using I2.Loc;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Text))]
public class ConnectionQualityIndicator : MonoBehaviour
{
	public Color SlowColour;

	public Color PoorColour;

	public Text QualityText;

	private LobbyManager.ConnectionQuality lastConnectionQuality = LobbyManager.ConnectionQuality.GREAT;

	private void Awake()
	{
		QualityText.enabled = false;
	}

	private void Update()
	{
		LobbyManager.ConnectionQuality connectionQuality = LobbyManager.ConnectionQuality.GREAT;
		if (LobbyManager.instance.IsInOnlineGame)
		{
			connectionQuality = LobbyManager.instance.GetConnectionQuality();
		}
		if (connectionQuality == LobbyManager.ConnectionQuality.GOOD)
		{
			connectionQuality = LobbyManager.ConnectionQuality.GREAT;
		}
		if (connectionQuality != lastConnectionQuality)
		{
			lastConnectionQuality = connectionQuality;
			switch (connectionQuality)
			{
			case LobbyManager.ConnectionQuality.POOR:
				QualityText.enabled = true;
				QualityText.text = "● " + ScriptLocalization.Network.Poor_Connection;
				QualityText.color = PoorColour;
				break;
			case LobbyManager.ConnectionQuality.SLOW:
				QualityText.enabled = true;
				QualityText.text = "● " + ScriptLocalization.Network.Poor_Connection;
				QualityText.color = SlowColour;
				break;
			case LobbyManager.ConnectionQuality.GOOD:
			case LobbyManager.ConnectionQuality.GREAT:
				QualityText.enabled = false;
				break;
			}
		}
	}
}
