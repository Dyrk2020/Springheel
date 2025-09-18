using UnityEngine;

public class CrumbleBlockDamageLevelSetter : MonoBehaviour
{
	public PickableBlock pickableBlock;

	public SpriteRenderer spriteRenderer;

	public int forcedDamageLevel;

	private void LateUpdate()
	{
		CrumblingBlockScriptableObject artReference = ((CrumblingBlock)pickableBlock.placeablePrefab).artReference;
		switch (forcedDamageLevel)
		{
		case 0:
			spriteRenderer.sprite = artReference.hold;
			break;
		case 1:
			spriteRenderer.sprite = artReference.broken1;
			break;
		case 2:
			spriteRenderer.sprite = artReference.broken2;
			break;
		}
	}
}
