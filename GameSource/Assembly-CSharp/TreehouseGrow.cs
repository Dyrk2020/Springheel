using UnityEngine;

public class TreehouseGrow : MonoBehaviour
{
	public int TreeState;

	public GameObject[] enableBelow;

	public GameObject[] enableExact;

	private void Start()
	{
	}

	private void Update()
	{
	}

	public void SetNewState(int newState)
	{
		if (newState > enableBelow.Length - 1)
		{
			TreeState = enableBelow.Length - 1;
			Debug.LogError("Treehouse being set to treehouse state that is too large and does not exist = " + newState);
		}
		else
		{
			TreeState = newState;
		}
		for (int i = 0; i < enableBelow.Length; i++)
		{
			if (i <= TreeState)
			{
				enableBelow[i].SetActive(value: true);
			}
			else
			{
				enableBelow[i].SetActive(value: false);
			}
			if (i == TreeState)
			{
				enableExact[i].SetActive(value: true);
			}
			else
			{
				enableExact[i].SetActive(value: false);
			}
		}
	}
}
