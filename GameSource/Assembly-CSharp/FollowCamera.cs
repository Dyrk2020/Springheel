using UnityEngine;

public class FollowCamera : MonoBehaviour
{
	public float horizontalBuffer;

	public float verticalBuffer;

	public float followSpeed;

	public Transform target;

	public Bounds boundary;

	private float aspectRatio;

	public float width;

	public float height;

	private float targetSpeed;

	private bool followTarget = true;

	public float Width => width;

	public float Height => height;

	private void Start()
	{
		float z = base.transform.position.z;
		if (target != null)
		{
			base.transform.position = target.position;
			base.transform.Translate(0f, 0f, z);
		}
		aspectRatio = Camera.main.aspect;
		height = Camera.main.orthographicSize;
		width = height * aspectRatio;
	}

	private void FixedUpdate()
	{
		if (target == null)
		{
			return;
		}
		Vector3 position = base.transform.position;
		float x = target.position.x - position.x;
		float y = target.position.y - position.y;
		Vector2 vector = new Vector2(x, y);
		float magnitude = vector.magnitude;
		if (targetSpeed > 0f)
		{
			vector.Normalize();
			if (targetSpeed * Time.unscaledDeltaTime > magnitude)
			{
				vector *= magnitude;
				targetSpeed = followSpeed;
			}
			else
			{
				vector *= targetSpeed * Time.unscaledDeltaTime;
			}
		}
		if (followTarget)
		{
			position += (Vector3)vector;
		}
		if (position.x - width < boundary.min.x)
		{
			position.x = boundary.min.x + width;
		}
		else if (position.x + width > boundary.max.x)
		{
			position.x = boundary.max.x - width;
		}
		if (position.y - height < boundary.min.y)
		{
			position.y = boundary.min.y + height;
		}
		else if (position.y + height > boundary.max.y)
		{
			position.y = boundary.max.y - height;
		}
		base.transform.position = position;
	}

	public void SetBounds(Bounds bounds)
	{
		boundary = bounds;
	}

	public void SetTarget(Transform target, float speed = 0f)
	{
		this.target = target;
		targetSpeed = speed;
	}

	public void AllowFollow(bool follow)
	{
		followTarget = follow;
	}
}
