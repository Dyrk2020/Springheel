public class WaitForConditionTweener : Tweener
{
	public delegate bool ConditionCheckCallback();

	public ConditionCheckCallback callback;

	public WaitForConditionTweener(ConditionCheckCallback callback)
	{
		this.callback = callback;
	}

	public override bool IsDone()
	{
		return callback();
	}
}
