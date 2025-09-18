using UnityEngine;

public class CreditEnabler : MonoBehaviour
{
	public GameObject CreditsAnimator;

	private float secondsToWait = 2f;

	private void Update()
	{
		secondsToWait -= Time.unscaledDeltaTime;
		if (secondsToWait <= 0f)
		{
			CreditsAnimator.SetActive(value: true);
			Object.Destroy(this);
		}
	}
}
