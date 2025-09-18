using UnityEngine;

public class blackholeBurst : MonoBehaviour
{
	protected Animator animator;

	protected SpriteRenderer spriteRenderer;

	public Character target;

	public Blackhole parentBlackhole;

	public Color[] colors;

	public float stopTracking;

	protected bool followChr;

	private void Start()
	{
		animator = GetComponent<Animator>();
		spriteRenderer = GetComponent<SpriteRenderer>();
		spriteRenderer.sortingOrder = 101;
		switch (Random.Range(0, 4))
		{
		case 1:
			animator.SetTrigger("B");
			break;
		case 2:
			animator.SetTrigger("C");
			break;
		case 3:
			animator.SetTrigger("D");
			break;
		default:
			animator.SetTrigger("A");
			break;
		case 0:
			break;
		}
		if ((bool)target)
		{
			Color color = Color.black;
			GameSettings.animalColors[] characterColors = GameSettings.GetInstance().characterColors;
			for (int i = 0; i < characterColors.Length; i++)
			{
				GameSettings.animalColors animalColors = characterColors[i];
				if (animalColors.type == target.CharacterSprite)
				{
					color = ((!(Random.value > 0.2f)) ? animalColors.secondaryColor : animalColors.mainColor);
					break;
				}
			}
			spriteRenderer.color = color;
		}
		followChr = true;
	}

	private void Update()
	{
		if (target != null)
		{
			Vector3 vector = target.transform.position - base.transform.position;
			if (vector.sqrMagnitude < stopTracking && followChr)
			{
				float num = Mathf.Atan2(vector.y, vector.x) * 57.29578f;
				base.transform.localRotation = Quaternion.Euler(0f, 0f, num + 180f);
				base.transform.localScale = new Vector3(Mathf.Clamp(vector.magnitude * GameSettings.GetInstance().blackholeBurstScale, 0.2f, 1.5f), 1f, 1f);
			}
			else
			{
				followChr = false;
			}
		}
		if (target == null || target.Dying)
		{
			DeleteMe();
		}
	}

	private void DeleteMe()
	{
		parentBlackhole.blackholeBurstFilled = false;
		Object.Destroy(base.gameObject);
	}
}
