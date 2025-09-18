using UnityEngine;

public static class Util_Vector3
{
	public static Vector3 Div(this Vector3 self, Vector3 rhs)
	{
		return new Vector3(self.x / rhs.x, self.y / rhs.y, self.z / rhs.z);
	}

	public static Vector3 Clamp(this Vector3 self, Vector3 min, Vector3 max)
	{
		return new Vector3(Mathf.Clamp(self.x, min.x, max.x), Mathf.Clamp(self.y, min.y, max.y), Mathf.Clamp(self.z, min.z, max.z));
	}

	public static Vector3 MakeVector3(this float self)
	{
		return new Vector3(self, self, self);
	}
}
