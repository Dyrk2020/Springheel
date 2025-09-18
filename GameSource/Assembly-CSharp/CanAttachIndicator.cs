using UnityEngine;

public class CanAttachIndicator : MonoBehaviour
{
	public HoneyPiece honeyPiece;

	private SpriteRenderer spriteRenderer;

	protected Color initialColor;

	protected float currentAlpha;

	public float changeSpeed = 0.01f;

	private void Start()
	{
		if (honeyPiece == null)
		{
			Debug.LogWarning("CanAttachIndicator: honeyPiece was not set!");
			honeyPiece = GetComponentInParent<HoneyPiece>();
		}
		spriteRenderer = GetComponent<SpriteRenderer>();
		initialColor = spriteRenderer.color;
		setAlpha(0f);
	}

	private void Update()
	{
		bool flag = honeyPiece.Enabled && !honeyPiece.Active && !honeyPiece.HasReverseAttachment && honeyPiece.Group != null && honeyPiece.Group.TopParent != null && honeyPiece.Group.TopParent.IsMobileBlock;
		if (honeyPiece.Placed && flag)
		{
			spriteRenderer.enabled = true;
			if (currentAlpha < initialColor.a)
			{
				setAlpha(Mathf.MoveTowards(currentAlpha, initialColor.a, changeSpeed * Time.deltaTime * 0.25f));
			}
		}
		else if (currentAlpha > 0f)
		{
			setAlpha(Mathf.MoveTowards(currentAlpha, 0f, changeSpeed * Time.deltaTime * 2f));
			if (currentAlpha < 0.01f)
			{
				spriteRenderer.enabled = false;
			}
		}
	}

	protected void setAlpha(float newAlpha)
	{
		currentAlpha = newAlpha;
		spriteRenderer.color = new Color(initialColor.r, initialColor.g, initialColor.b, newAlpha);
	}
}
