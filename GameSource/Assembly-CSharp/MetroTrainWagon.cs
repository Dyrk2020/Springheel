using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CheckColliding))]
public class MetroTrainWagon : MonoBehaviour
{
	public MetroTrainMove metroMove;

	public CheckColliding checkColliding;

	public float velocityInheritanceRatio = 1f;

	private List<Character> characterOnWagonLastFrame = new List<Character>();

	private List<Character> leavingWagonChars = new List<Character>();

	private void FixedUpdate()
	{
		if (checkColliding == null || metroMove == null)
		{
			return;
		}
		leavingWagonChars.Clear();
		for (int i = 0; i < characterOnWagonLastFrame.Count; i++)
		{
			Character character = characterOnWagonLastFrame[i];
			bool flag = false;
			foreach (Character collidingCharacter in checkColliding.CollidingCharacters)
			{
				if (collidingCharacter == character)
				{
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				leavingWagonChars.Add(character);
			}
		}
		foreach (Character leavingWagonChar in leavingWagonChars)
		{
			leavingWagonChar.AddImpulse(metroMove.Velocity * velocityInheritanceRatio);
		}
		characterOnWagonLastFrame.Clear();
		characterOnWagonLastFrame.AddRange(checkColliding.CollidingCharacters);
	}
}
