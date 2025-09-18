using System.Collections;
using UnityEngine;

public class ArrowHitDebris : MonoBehaviour
{
	public AnimationCurve easeSettle;

	public float DestroyAfterSeconds = 10f;

	public void startSettleHitDebirs(Vector3 startLocal, Vector3 endLocalDiff, Quaternion startRotation, Quaternion EndrotationDiff, float timeOfSettle)
	{
		StartCoroutine(settleHitDebris(startLocal, endLocalDiff, startRotation, EndrotationDiff, timeOfSettle));
		StartCoroutine(destroyAfterAwhile());
	}

	public IEnumerator settleHitDebris(Vector3 startLocal, Vector3 endLocalDiff, Quaternion startRotation, Quaternion EndrotationDiff, float timeOfSettle)
	{
		for (float i = Time.deltaTime; i < timeOfSettle; i += Time.deltaTime)
		{
			float t = easeSettle.Evaluate(i / timeOfSettle);
			base.transform.localPosition = Vector3.Lerp(startLocal, startLocal + endLocalDiff, t);
			base.transform.localRotation = Quaternion.Lerp(startRotation, startRotation * EndrotationDiff, t);
			yield return null;
		}
	}

	public IEnumerator destroyAfterAwhile()
	{
		yield return new WaitForSeconds(DestroyAfterSeconds);
		Object.Destroy(base.gameObject);
	}
}
