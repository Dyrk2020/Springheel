public class RetryButton : PickableButton
{
	public BaseScoreboard Scoreboard;

	private bool onHost;

	private float initialHoverScale;

	protected override void Awake()
	{
		base.Awake();
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
		if (onHost)
		{
			Scoreboard.Hide();
		}
	}

	public void SetOnHost(bool onHost)
	{
		this.onHost = onHost;
		if (onHost)
		{
			SetAlpha(1f);
			hoveredScaleModifier = initialHoverScale;
		}
		else
		{
			SetAlpha(0f);
			hoveredScaleModifier = 0f;
			outlineHighlight.a = 0f;
		}
	}
}
