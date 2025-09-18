using UnityEngine;

namespace GameEvent;

public class SetpieceColorChangeEvent : GameEvent
{
	public readonly Color NewColor;

	public SetpieceColorChangeEvent(Color newColor)
	{
		NewColor = newColor;
	}
}
