using System.Collections.Generic;
using Dreamteck.Splines;
using UnityEngine;

[ExecuteAlways]
public class RollerCoasterTrain : MonoBehaviour
{
	[SerializeField]
	public List<SplinePositioner> carPositions;

	public float offsetAmount;

	public float mainDistances;

	private void Start()
	{
	}

	private void Update()
	{
		int num = 0;
		foreach (SplinePositioner carPosition in carPositions)
		{
			carPosition.SetDistance(mainDistances + offsetAmount * (float)num);
			num++;
		}
	}
}
