using System.Collections;
using UnityEngine;

[ExecuteInEditMode]
public class EditorCoroutineRunner : MonoBehaviour
{
	public IEnumerator coroutine;

	private void OnEnable()
	{
	}

	private void OnEditorUpdate()
	{
		if (coroutine != null)
		{
			coroutine.MoveNext();
		}
	}
}
