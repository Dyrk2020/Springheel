using UnityEngine;

public class walljumptest : MonoBehaviour
{
	public enum TestType
	{
		WallSnag,
		DiagonalSnag
	}

	public TestType testType;

	public Object plankPrefab;

	public bool runTest;

	public Character character;

	public bool continuousCollision;

	private GameObject[] planks;

	private bool testRunning;

	private float timer;

	private int phase;

	public float wallSnagPhase0Time = 0.5f;

	public float wallSnagPhase1Time = 0.62f;

	public float wallSnagPhase2Time = 1f;

	public float diagSnagPhase0Time = 0.5f;

	public float diagSnagPhase1Time = 0.5f;

	public float diagSnagPhase2Time = 0.5f;

	private Vector3 startPos;

	private void Start()
	{
		continuousCollision = character.GetComponent<Rigidbody2D>().collisionDetectionMode == CollisionDetectionMode2D.Continuous;
		planks = new GameObject[2]
		{
			(GameObject)Object.Instantiate(plankPrefab),
			(GameObject)Object.Instantiate(plankPrefab)
		};
		planks[0].transform.rotation = Quaternion.AngleAxis(90f, Vector3.forward);
		planks[0].transform.position = new Vector3(15.5f, -13.5f, 0f);
		planks[1].transform.rotation = Quaternion.AngleAxis(90f, Vector3.forward);
		planks[1].transform.position = new Vector3(15.5f, -18.5f, 0f);
	}

	private void FixedUpdate()
	{
		if (continuousCollision)
		{
			character.GetComponent<Rigidbody2D>().collisionDetectionMode = CollisionDetectionMode2D.Continuous;
		}
		else
		{
			character.GetComponent<Rigidbody2D>().collisionDetectionMode = CollisionDetectionMode2D.None;
		}
		switch (testType)
		{
		case TestType.WallSnag:
			planks[0].SetActive(value: true);
			planks[1].SetActive(value: true);
			startPos = new Vector3(17f, -4f, 0f);
			break;
		case TestType.DiagonalSnag:
			planks[0].SetActive(value: true);
			planks[1].SetActive(value: true);
			startPos = new Vector3(17f, -4f, 0f);
			break;
		}
		if (runTest && !testRunning)
		{
			testRunning = true;
			runTest = false;
			character.transform.localPosition = startPos;
			timer = 0f;
			phase = 0;
			character.jump = false;
			character.right = 0f;
		}
		if (!testRunning)
		{
			return;
		}
		if (testType == TestType.WallSnag)
		{
			switch (phase)
			{
			case 0:
				if (timer > wallSnagPhase0Time)
				{
					phase = 1;
					timer -= wallSnagPhase0Time;
				}
				break;
			case 1:
				character.right = 1f;
				character.sprint = true;
				character.sprintDown = true;
				if (timer > wallSnagPhase1Time)
				{
					phase = 2;
					timer -= wallSnagPhase1Time;
					character.jump = true;
				}
				break;
			case 2:
				if (timer > wallSnagPhase2Time)
				{
					testRunning = false;
				}
				break;
			}
		}
		else
		{
			switch (phase)
			{
			case 0:
				if (timer > diagSnagPhase0Time)
				{
					phase = 1;
					timer -= diagSnagPhase0Time;
				}
				break;
			case 1:
				character.right = 1f;
				character.sprint = true;
				character.sprintDown = true;
				if (timer > diagSnagPhase1Time)
				{
					phase = 2;
					timer -= diagSnagPhase1Time;
					character.jump = true;
				}
				break;
			case 2:
				if (timer > diagSnagPhase2Time)
				{
					character.jump = !character.jump;
				}
				if (timer > 5f)
				{
					testRunning = false;
				}
				break;
			}
		}
		timer += Time.deltaTime;
	}
}
