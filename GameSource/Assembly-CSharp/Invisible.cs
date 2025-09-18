using UnityEngine;

public class Invisible : MonoBehaviour
{
	private void Start()
	{
		Renderer component = GetComponent<Renderer>();
		if (component != null)
		{
			component.enabled = false;
		}
	}

	private void Update()
	{
	}
}
