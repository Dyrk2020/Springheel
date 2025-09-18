using UnityEngine;

public class LevelStartIndicator : MonoBehaviour
{
	public SpriteRenderer glowingLight;

	public GameObject startZoneText_Box;

	public Animator lightAnimator;

	public void Show()
	{
		startZoneText_Box.SetActive(value: true);
		lightAnimator.SetBool("On", value: true);
	}

	public void Hide()
	{
		startZoneText_Box.SetActive(value: false);
		lightAnimator.SetBool("On", value: false);
	}
}
