using UnityEngine;

public class PunchingBlockHazard : MonoBehaviour
{
	public PunchingBlock punchingBlock;

	public float strength = 10f;

	public float teleportDist = 5.5f;

	public float raycastStartDist = 0.6f;

	public float characterThickness = 0.375f;

	public LayerMask wallRaycastMask;

	private void OnTriggerEnter2D(Collider2D collider)
	{
		Character componentInParent = collider.GetComponentInParent<Character>();
		if (!(componentInParent != null))
		{
			return;
		}
		bool dying = componentInParent.Dying;
		componentInParent.KillCharacter("PunchingBlock", deathFreezeOn: true, punchingBlock.placedByPlayerNumber);
		if (componentInParent.AffectedByImpulse)
		{
			return;
		}
		AkSoundEngine.PostEvent("SFX_Pieces_Boxing_Glove_PlayerHit", punchingBlock.gameObject);
		Vector3 right = base.transform.right;
		Vector3 up = base.transform.up;
		Vector3 position = punchingBlock.transform.position;
		Vector3 lhs = componentInParent.transform.position - position;
		float num = Vector3.Dot(lhs, right);
		float num2 = Vector3.Dot(lhs, up);
		Vector3 vector = right;
		if (componentInParent.OnGround)
		{
			vector += Vector3.up * 0.5f;
			vector.Normalize();
		}
		else
		{
			float num3 = Mathf.Clamp01(Mathf.Abs(num2 * 2f));
			if (num2 >= 0f)
			{
				vector += up * 0.5f * num3;
			}
			else
			{
				vector -= up * 0.5f * num3;
			}
			vector.Normalize();
		}
		if (!dying)
		{
			float num4 = Mathf.Max(raycastStartDist, num - characterThickness);
			RaycastHit2D raycastHit2D = Physics2D.Raycast(base.transform.position + right * num4, right, teleportDist - num4, wallRaycastMask.value);
			float magnitude = teleportDist;
			if (raycastHit2D.collider != null)
			{
				magnitude = (raycastHit2D.point.ToVector3() - position - right * characterThickness).magnitude;
			}
			componentInParent.PositionCharacter(base.transform.position + up * num2 + right * magnitude);
			componentInParent.ForceDeathVelocity(vector * strength);
		}
		else
		{
			componentInParent.AddImpulse(vector * strength, 0.1f);
		}
	}
}
