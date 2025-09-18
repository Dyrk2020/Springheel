using UnityEngine;

public class PlayerScore : MonoBehaviour
{
	public Checkbox CheckboxPrefab;

	public int MaxScore = 5;

	public Checkbox[] Checkboxes;

	public float Spacing;

	public SpriteRenderer Icon;

	public SpriteRenderer Name;

	protected int score;

	private void Start()
	{
	}

	private void Update()
	{
	}

	public void SetMaxScore(int maxScore)
	{
		MaxScore = maxScore;
		Checkboxes = new Checkbox[MaxScore];
		for (int i = 0; i != MaxScore; i++)
		{
			Vector3 position = base.transform.position;
			position.x += (float)i * Spacing;
			GameObject gameObject = Object.Instantiate(CheckboxPrefab.gameObject, position, Quaternion.identity);
			gameObject.transform.parent = base.transform;
			Checkboxes[i] = gameObject.GetComponent<Checkbox>();
		}
	}

	public void IncrementScore(int amount = 1)
	{
		score += amount;
		score = Mathf.Clamp(score, 0, MaxScore);
		SetScore(score);
	}

	public void SetScore(int score)
	{
		if (score >= 0 && score <= MaxScore)
		{
			this.score = score;
			for (int i = 0; i != score; i++)
			{
				Checkboxes[i].Checked = true;
			}
			for (int j = score; j != MaxScore; j++)
			{
				Checkboxes[j].Checked = false;
			}
		}
	}

	public int GetScore()
	{
		return score;
	}

	public void Show()
	{
		Checkbox[] checkboxes = Checkboxes;
		for (int i = 0; i < checkboxes.Length; i++)
		{
			checkboxes[i].Show();
		}
	}

	public void Hide()
	{
		Checkbox[] checkboxes = Checkboxes;
		for (int i = 0; i < checkboxes.Length; i++)
		{
			checkboxes[i].Hide();
		}
	}
}
