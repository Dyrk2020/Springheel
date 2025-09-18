using UnityEngine;

public class FavouriteButton : PickableButton
{
	public Color DefaultColour;

	public Color ClickedColour;

	private bool favourite;

	private string snapshotName;

	private string snapshotCode;

	public Sprite filledStar;

	public Sprite EmptyStar;

	public ChallengeScoreboard scoreboard;

	private Vector3 initialScaleMemory;

	public float FavSetScale = 1.2f;

	protected override void Awake()
	{
		base.Awake();
		initialScaleMemory = initialScale;
	}

	protected override void Start()
	{
		base.Start();
		Enable();
	}

	public override void OnAccept(PickCursor pickCursor)
	{
		base.OnAccept(pickCursor);
		SetFavourite(!favourite);
		if (favourite)
		{
			StatTracker.Instance.GetSaveFileDataForMainUser().AddFavoriteSnapshotCode(snapshotName, snapshotCode);
			AkSoundEngine.PostEvent("UI_Ingame_ChallengeMode_Favorite", base.gameObject);
		}
		else
		{
			StatTracker.Instance.GetSaveFileDataForMainUser().RemoveFavoriteSnapshotCode(snapshotName, snapshotCode);
		}
	}

	public void SetSnapshot(string name, string code)
	{
		snapshotName = name;
		snapshotCode = code;
	}

	public void SetFavourite(bool favourite)
	{
		this.favourite = favourite;
		if (favourite)
		{
			if (image != null)
			{
				image.color = ClickedColour;
				image.sprite = filledStar;
			}
			if (buttonText != null)
			{
				buttonText.color = ClickedColour;
			}
			initialScale = initialScaleMemory * FavSetScale;
		}
		else
		{
			if (image != null)
			{
				image.color = DefaultColour;
				image.sprite = EmptyStar;
			}
			if (buttonText != null)
			{
				buttonText.color = DefaultColour;
			}
			initialScale = initialScaleMemory;
		}
	}
}
