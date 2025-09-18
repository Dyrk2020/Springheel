using UnityEngine;

public static class DebugUtil
{
	public static void DrawDebugX(Vector3 position, float size, Color color, float duration = 0f)
	{
		Debug.DrawLine(position + Vector3.up * size, position + Vector3.down * size, color, duration);
		Debug.DrawLine(position + Vector3.left * size, position + Vector3.right * size, color, duration);
	}

	public static void DrawCircle(Vector3 center, Vector3 normal, float radius, Color color, float duration = 0f)
	{
		int num = 32;
		float num2 = 360f / (float)num;
		Quaternion quaternion = Quaternion.LookRotation(normal);
		Vector3 start = center + quaternion * Vector3.right * radius;
		for (int i = 1; i <= num; i++)
		{
			Vector3 vector = center + quaternion * Quaternion.Euler(0f, num2 * (float)i, 0f) * Vector3.right * radius;
			Debug.DrawLine(start, vector, color, duration);
			start = vector;
		}
	}
}
