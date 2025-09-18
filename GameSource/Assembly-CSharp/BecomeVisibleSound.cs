using UnityEngine;
using UnityEngine.Events;

public class BecomeVisibleSound : MonoBehaviour
{
	public string onVisibleString;

	public GameObject targetGameobject;

	public UnityEvent OnVisible;

	private void Start()
	{
		if (targetGameobject == null)
		{
			targetGameobject = base.gameObject;
		}
	}

	private void OnBecameVisible()
	{
		if (!onVisibleString.NullOrEmpty())
		{
			AkSoundEngine.PostEvent(onVisibleString, targetGameobject);
		}
		if (OnVisible != null)
		{
			OnVisible.Invoke();
		}
	}
}
