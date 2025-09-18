using System.Collections;
using UnityEngine;

public class ChildDestroyer : MonoBehaviour
{
	private void Start()
	{
		StartCoroutine("Cleanup");
	}

	private IEnumerator Cleanup()
	{
		while (true)
		{
			yield return new WaitForEndOfFrame();
			Transform[] componentsInChildren = GetComponentsInChildren<Transform>();
			for (int i = 1; i != componentsInChildren.Length; i++)
			{
				Object.Destroy(componentsInChildren[i].gameObject);
			}
		}
	}
}
