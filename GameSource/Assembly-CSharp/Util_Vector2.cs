using UnityEngine;

public static class Util_Vector2
{
	public static Vector3 ToVector3(this Vector2 self)
	{
		return new Vector3(self.x, self.y);
	}
}
