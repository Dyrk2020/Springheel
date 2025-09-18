using GameEvent;
using UnityEngine;
using UnityEngine.UI;

public class ConnectionQualityBars : PickableOnlineSettingButton, IGameEventListener
{
	public Color PoorColour;

	public Color SlowColour;

	public Color GoodColour;

	public Image QualityImage;

	public Sprite[] QualitySprites;

	public LobbyManager.ConnectionQuality Quality;

	protected override void Start()
	{
		base.Start();
		if (QualityImage == null)
		{
			QualityImage = GetComponentInChildren<Image>();
		}
	}

	protected override void Update()
	{
		base.Update();
		switch (Quality)
		{
		case LobbyManager.ConnectionQuality.POOR:
			QualityImage.sprite = QualitySprites[0];
			QualityImage.color = PoorColour;
			break;
		case LobbyManager.ConnectionQuality.SLOW:
			QualityImage.sprite = QualitySprites[1];
			QualityImage.color = SlowColour;
			break;
		case LobbyManager.ConnectionQuality.GOOD:
			QualityImage.sprite = QualitySprites[2];
			QualityImage.color = GoodColour;
			break;
		case LobbyManager.ConnectionQuality.GREAT:
			QualityImage.sprite = QualitySprites[3];
			QualityImage.color = GoodColour;
			break;
		}
	}
}
