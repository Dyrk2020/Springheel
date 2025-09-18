public class RedditButton : PickableButton
{
	public ChallengeScoreboard Scoreboard;

	private bool onHost;

	protected override void Start()
	{
		base.Start();
		Enable();
	}

	public override void OnAccept(PickCursor pickCursor)
	{
		base.OnAccept(pickCursor);
		Scoreboard.OnClickChallengeRedditShare();
	}
}
