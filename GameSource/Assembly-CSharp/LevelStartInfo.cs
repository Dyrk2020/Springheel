using UnityEngine;

public class LevelStartInfo : MonoBehaviour
{
	public Transform StartPoint;

	public Transform StartPoint2;

	public Transform[] SpectatorStart;

	public Transform SpectatorStartParent;

	public Transform CursorSpawnPoint;

	public LevelStartIndicator Indicator;

	public void ApplyToLevel(Level level)
	{
		level.StartPoint = StartPoint;
		level.StartPoint2 = StartPoint2;
		level.SpectatorStart = SpectatorStart;
		level.SpectatorStartParent = SpectatorStartParent;
		level.CursorSpawnPoint = CursorSpawnPoint;
		Indicator.Show();
	}

	public void Hide()
	{
		Indicator.Hide();
	}
}
