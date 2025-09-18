using System.Collections;
using UnityEngine;

public class LabFlagBlocker : MonoBehaviour
{
	protected Renderer rend;

	protected Collider2D coll;

	public float intervalTime;

	private void Start()
	{
		rend = GetComponent<Renderer>();
		coll = GetComponent<Collider2D>();
		StartCoroutine(FlagBlocker());
	}

	private IEnumerator FlagBlocker()
	{
		bool onOff = true;
		while (true)
		{
			rend.enabled = onOff;
			coll.enabled = onOff;
			yield return new WaitForSeconds(intervalTime);
			onOff = !onOff;
		}
	}
}
