using UnityEngine;

public class BitSummitBuildIncluder : MonoBehaviour
{
	private void Start()
	{
		if (!GameSettings.GetInstance().BitSummitBuild)
		{
			base.gameObject.SetActive(value: false);
		}
	}
}
