using UnityEngine;

public class BitSummitBuildExcluder : MonoBehaviour
{
	private void Start()
	{
		if (GameSettings.GetInstance().BitSummitBuild)
		{
			base.gameObject.SetActive(value: false);
		}
	}
}
