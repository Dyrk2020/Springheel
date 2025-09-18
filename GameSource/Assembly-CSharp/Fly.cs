using UnityEngine;

public class Fly : MonoBehaviour
{
	public Character followTarget;

	private Vector3 targetPosition;

	public AnimationCurve flyFollowSpeed;

	public GrassAnimator customAnimator;

	public Sprite[] fly1;

	public Sprite[] fly2;

	public Sprite[] fly3;

	public Sprite[] fly4;

	public SpriteRenderer sr;

	public Vector3 followOffset;

	private void Update()
	{
		if (followTarget != null)
		{
			targetPosition = followTarget.transform.position + followOffset;
			float sqrMagnitude = (targetPosition - base.transform.position).sqrMagnitude;
			base.transform.position = Vector3.MoveTowards(base.transform.position, targetPosition, flyFollowSpeed.Evaluate(sqrMagnitude) * Time.deltaTime);
		}
	}

	public void Initialize(Character followChr)
	{
		followTarget = followChr;
		switch (Random.Range(1, 7))
		{
		case 1:
			customAnimator.grassSpriteAnimation = fly1;
			break;
		case 2:
			customAnimator.grassSpriteAnimation = fly2;
			break;
		case 3:
			customAnimator.grassSpriteAnimation = fly3;
			break;
		case 4:
			customAnimator.grassSpriteAnimation = fly4;
			break;
		default:
			customAnimator.grassSpriteAnimation = fly1;
			break;
		}
		customAnimator.Start();
	}
}
