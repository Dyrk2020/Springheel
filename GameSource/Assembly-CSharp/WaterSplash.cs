using UnityEngine;

public class WaterSplash : MonoBehaviour
{
	private Animator animator;

	private SpriteRenderer spriteRenderer;

	public bool Splashing { get; protected set; }

	private void Start()
	{
		animator = GetComponentInChildren<Animator>();
		spriteRenderer = GetComponentInChildren<SpriteRenderer>();
		doneSplashing();
	}

	public void Splash()
	{
		if (spriteRenderer != null)
		{
			spriteRenderer.enabled = true;
		}
		if (animator != null)
		{
			animator.SetTrigger("Splash");
		}
		Splashing = true;
		AkSoundEngine.PostEvent("SFX_Level_Iceberg_Water_Splash", base.gameObject);
	}

	private void doneSplashing()
	{
		Splashing = false;
		if (spriteRenderer != null)
		{
			spriteRenderer.enabled = false;
		}
	}
}
