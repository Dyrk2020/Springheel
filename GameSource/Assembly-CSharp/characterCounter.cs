using System.Collections.Generic;
using UnityEngine;

public class characterCounter : MonoBehaviour
{
	public int overlaps;

	public List<Character> hoverCharacters = new List<Character>();

	private void Update()
	{
		overlaps = hoverCharacters.Count;
	}

	private void OnTriggerStay2D(Collider2D c)
	{
		Character componentInParent = c.gameObject.GetComponentInParent<Character>();
		if (componentInParent != null && !hoverCharacters.Contains(componentInParent))
		{
			hoverCharacters.Add(componentInParent);
		}
	}

	private void OnTriggerExit2D(Collider2D c)
	{
		Character componentInParent = c.gameObject.GetComponentInParent<Character>();
		if (componentInParent != null && hoverCharacters.Contains(componentInParent))
		{
			hoverCharacters.Remove(componentInParent);
		}
	}
}
