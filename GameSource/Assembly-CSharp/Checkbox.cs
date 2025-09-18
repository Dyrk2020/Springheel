using UnityEngine;

public class Checkbox : MonoBehaviour
{
	public Sprite[] Boxes;

	public Sprite[] Checks;

	private bool checkd;

	public SpriteRenderer check;

	public Animator checkAnim;

	public bool Checked
	{
		get
		{
			return checkd;
		}
		set
		{
			check.enabled = checkd;
			if (checkAnim != null && value)
			{
				if (!checkd)
				{
					AkSoundEngine.PostEvent("UI_InGame_PointAwarded", base.gameObject);
				}
				checkAnim.SetTrigger("Check");
			}
			checkd = value;
		}
	}

	private void Awake()
	{
		GetComponent<SpriteRenderer>().sprite = Boxes[Random.Range(0, Boxes.Length)];
		check.sprite = Checks[Random.Range(0, Checks.Length)];
		check.enabled = false;
	}

	private void Update()
	{
	}

	public void Show()
	{
		GetComponent<SpriteRenderer>().enabled = true;
		check.enabled = checkd;
	}

	public void Hide()
	{
		GetComponent<SpriteRenderer>().enabled = false;
		check.enabled = false;
	}
}
