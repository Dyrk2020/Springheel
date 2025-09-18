using System.Collections;
using UnityEngine.Events;

public static class AsyncCommand
{
	public delegate bool ConditionCheckDelegate();

	public static IEnumerator WaitForCondition(ConditionCheckDelegate condition, UnityAction onConditionMet)
	{
		if (!condition())
		{
			yield return null;
			while (!condition())
			{
				yield return null;
			}
		}
		onConditionMet();
	}
}
