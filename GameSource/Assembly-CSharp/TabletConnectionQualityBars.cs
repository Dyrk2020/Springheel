using UnityEngine;
using UnityEngine.UI;

public class TabletConnectionQualityBars : MonoBehaviour
{
	public Color PoorColour;

	public Color SlowColour;

	public Color GoodColour;

	public Image QualityImage;

	public Sprite[] QualitySprites;

	public LobbyManager.ConnectionQuality Quality;

	private void Start()
	{
		if (QualityImage == null)
		{
			QualityImage = GetComponentInChildren<Image>();
		}
	}

	private void Update()
	{
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
