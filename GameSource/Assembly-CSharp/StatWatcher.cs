using UnityEngine;

public class StatWatcher : MonoBehaviour
{
	[SerializeField]
	public StatTracker stats;

	private void Start()
	{
		stats = StatTracker.Instance;
		Object.DontDestroyOnLoad(this);
	}

	private void Update()
	{
	}
}
