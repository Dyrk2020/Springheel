using UnityEngine;

public class puckSprite : MonoBehaviour
{
	public float initialVelocityUp;

	public float gravity;

	public float hitFreezeTime;

	public float destroyAfterTime;

	public Vector3 normalOfCollision;

	public float rotationSpeed;

	public float bounciness;

	protected Vector3 velocity;

	protected bool hitFreeze = true;

	protected float hitFreezeTimer;

	public SpriteRenderer spriteRenderer;

	private void Start()
	{
		velocity = new Vector3(0f, initialVelocityUp, 0f);
		velocity += normalOfCollision * Random.Range(-1f, 10f) * bounciness;
		rotationSpeed = Random.Range(-100f, 100f) * rotationSpeed;
	}

	private void Update()
	{
		hitFreezeTimer += Time.deltaTime;
		if (hitFreezeTimer > hitFreezeTime)
		{
			velocity += gravity * Vector3.up * Time.deltaTime;
			base.transform.position += velocity * Time.deltaTime;
			base.transform.Rotate(0f, 0f, rotationSpeed * Time.deltaTime);
		}
		if (hitFreezeTimer > destroyAfterTime && !spriteRenderer.isVisible)
		{
			Object.Destroy(base.gameObject);
		}
	}

	public void SetSortingLayer(int sortingLayerID, int sortingOrder)
	{
		spriteRenderer.sortingLayerID = sortingLayerID;
		spriteRenderer.sortingOrder = sortingOrder;
	}
}
