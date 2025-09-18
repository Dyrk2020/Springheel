using UnityEngine;

public class VoteButton : PickableButton
{
	[Range(-1f, 1f)]
	public int VoteScore;

	public VoteButton OppositeButton;

	public Color DefaultColour;

	public Color ClickedColour;

	private bool voted;

	private bool canVote;

	private float initialHoverScale;

	public ChallengeScoreboard scoreboard;

	private Vector3 initialScaleMemory;

	public float VoteSetScale = 1.2f;

	protected override void Awake()
	{
		base.Awake();
		initialScaleMemory = initialScale;
		initialHoverScale = hoveredScaleModifier;
	}

	protected override void Start()
	{
		base.Start();
		Enable();
	}

	public override void OnAccept(PickCursor pickCursor)
	{
		base.OnAccept(pickCursor);
		if (canVote)
		{
			scoreboard.CastVote(this);
		}
	}

	public void SetVote(bool vote)
	{
		if (!canVote)
		{
			return;
		}
		voted = vote;
		if (vote)
		{
			if (image != null)
			{
				image.color = ClickedColour;
			}
			if (buttonText != null)
			{
				buttonText.color = ClickedColour;
			}
			initialScale = initialScaleMemory * VoteSetScale;
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
			initialScale = initialScaleMemory;
		}
	}

	public void SetCanVote(bool canVote)
	{
		this.canVote = canVote;
		if (canVote)
		{
			SetAlpha(1f);
			hoveredScaleModifier = initialHoverScale;
			SetVote(voted);
		}
		else
		{
			SetAlpha(0.5f);
			hoveredScaleModifier = 0f;
			outlineHighlight.a = 0f;
			image.color = DefaultColour;
		}
	}
}
