using UnityEngine;

public class DestroyIfNotSwitch : MonoBehaviour
{
	private void Awake()
	{
		Object.DestroyImmediate(base.gameObject);
	}
}
