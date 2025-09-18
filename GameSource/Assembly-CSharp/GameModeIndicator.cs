using UnityEngine;
using UnityEngine.UI;

public class GameModeIndicator : MonoBehaviour
{
	public GameState.GameMode gameModeToIndicate;

	protected SpriteRenderer sprite;

	protected Image image;

	public Color OnColor;

	public Color OffColor;

	protected float CurrentColorFloat = 0.5f;

	protected Color CurrentColor;

	public float ChangeSpeed;

	private void Start()
	{
		sprite = GetComponent<SpriteRenderer>();
		image = GetComponent<Image>();
		base.gameObject.SetActive(GameState.ModeIsAllowed(gameModeToIndicate));
	}

	private void Update()
	{
		float num = 0f;
		num = ((gameModeToIndicate != GameSettings.GetInstance().GameMode) ? 0f : 1f);
		if (CurrentColorFloat != num)
		{
			CurrentColorFloat = Mathf.MoveTowards(CurrentColorFloat, num, ChangeSpeed * Time.deltaTime);
			CurrentColor = Color.Lerp(OffColor, OnColor, CurrentColorFloat);
			if ((bool)sprite)
			{
				sprite.color = CurrentColor;
			}
			if ((bool)image)
			{
				image.color = CurrentColor;
			}
		}
	}
}
