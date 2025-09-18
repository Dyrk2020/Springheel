using UnityEngine;

public class FlagButton : PickableButton
{
	public Color DefaultColour;

	public Color ClickedColour;

	public string snapshotName;

	public string snapshotCode;

	public ChallengeScoreboard challengeScoreboard;

	protected override void Awake()
	{
		base.Awake();
	}

	protected override void Start()
	{
		base.Start();
		Enable();
	}

	public override void OnAccept(PickCursor pickCursor)
	{
		base.OnAccept(pickCursor);
		challengeScoreboard.OnClickReport();
	}

	public void SetSnapshot(string name, string code)
	{
		snapshotName = name;
		snapshotCode = code;
	}

	public void SetFlagged(bool flagged)
	{
		if (flagged)
		{
			if (image != null)
			{
				image.color = ClickedColour;
			}
			if (buttonText != null)
			{
				buttonText.color = ClickedColour;
			}
		}
		else
		{
			if (image != null)
			{
				image.color = DefaultColour;
			}
			if (buttonText != null)
			{
				buttonText.color = DefaultColour;
			}
		}
	}
}
