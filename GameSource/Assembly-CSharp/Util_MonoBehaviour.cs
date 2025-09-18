using UnityEngine;
using UnityEngine.Events;

public static class Util_MonoBehaviour
{
	public static void StartCoroutineWithCondition(this MonoBehaviour self, AsyncCommand.ConditionCheckDelegate condition, UnityAction onConditionMet)
	{
		self.StartCoroutine(AsyncCommand.WaitForCondition(condition, onConditionMet));
	}
}
