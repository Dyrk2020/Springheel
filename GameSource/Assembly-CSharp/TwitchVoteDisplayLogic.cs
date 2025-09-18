using System.Collections;
using I2.Loc;
using UnityEngine;
using UnityEngine.UI;

public class TwitchVoteDisplayLogic : MonoBehaviour
{
	public CanvasGroup contentsCanvasGroup;

	public Text voteCountText;

	public Text voteCountLabelText;

	public Text voteItemName;

	public GameObject placeholderBox;

	public Transform animated;

	public Animator animator;

	private PickableBlock pickableBlock;

	public int lastVoteCount;

	public int lastPickableIndex = -1;

	public TwitchVoteListCanvasLogic VoteCanvas;

	public float animationTime;

	public AnimationCurve PartyBoxMoveX;

	public AnimationCurve PartyBoxMoveY;

	public AnimationCurve PartyBoxRotate;

	public bool Animating;

	private void Awake()
	{
		ShowContents(show: false);
	}

	private void Start()
	{
		VoteCanvas = GetComponentInParent<TwitchVoteListCanvasLogic>();
	}

	private void Update()
	{
	}

	public void ShowContents(bool show)
	{
		contentsCanvasGroup.alpha = (show ? 1 : 0);
		if (pickableBlock != null)
		{
			pickableBlock.gameObject.SetActive(show);
		}
	}

	public void SetItemName(string itemName, int pickableIndex)
	{
		if (lastPickableIndex == pickableIndex)
		{
			return;
		}
		string localizedShortName = TwitchTermMapper.GetLocalizedShortName(pickableIndex);
		if (localizedShortName != null)
		{
			voteItemName.text = localizedShortName;
		}
		else
		{
			voteItemName.text = itemName;
		}
		lastPickableIndex = pickableIndex;
		if (pickableBlock != null)
		{
			Object.Destroy(pickableBlock.gameObject);
			pickableBlock = null;
		}
		if (pickableIndex == -1)
		{
			return;
		}
		GameControl component = ((GameObject)TwitchChatController.instance.versusControllerPrefab).GetComponent<GameControl>();
		if (!(component != null) || pickableIndex < 0 || pickableIndex >= component.MetaList.AllBlockListLength())
		{
			return;
		}
		GameObject prefab = component.MetaList.GetPlaceableByIndex(pickableIndex).PickableBlock.gameObject;
		pickableBlock = placeholderBox.AddPrefabAsChild<PickableBlock>(prefab);
		pickableBlock.Enable(enable: true);
		pickableBlock.ChangeArtLayer("UI 3");
		pickableBlock.SpriteSortOrder.setSortOrder(210);
		float preferredWidth = placeholderBox.GetComponent<LayoutElement>().preferredWidth;
		Vector2 vector = Vector2.zero;
		Vector2 vector2 = Vector2.zero;
		Collider2D[] pickColliders = pickableBlock.PickColliders;
		for (int i = 0; i < pickColliders.Length; i++)
		{
			BoxCollider2D boxCollider2D = pickColliders[i] as BoxCollider2D;
			if ((bool)boxCollider2D)
			{
				Vector2 vector3 = boxCollider2D.size * 0.5f;
				Vector2 rhs = boxCollider2D.offset + vector3;
				Vector2 rhs2 = boxCollider2D.offset - vector3;
				vector = Vector2.Min(vector, rhs2);
				vector2 = Vector2.Max(vector2, rhs);
			}
		}
		Vector3 vector4 = vector2 - vector;
		float num = Mathf.Max(vector4.x, vector4.y, vector4.z);
		float num2 = preferredWidth / num;
		pickableBlock.transform.localScale = new Vector3(num2, num2, num2);
		Vector2 self = -(vector2 + vector) * 0.5f;
		self *= num2;
		pickableBlock.transform.localPosition = self.ToVector3();
		pickableBlock.DeactivateForTwitchVotePanelDisplay();
	}

	public void SetUILayer(string newLayerName, int OrderNumber)
	{
		if (pickableBlock != null)
		{
			pickableBlock.ChangeArtLayer(newLayerName);
			pickableBlock.SpriteSortOrder.setSortOrder(OrderNumber);
		}
	}

	public void SetVoteCount(int number)
	{
		if (lastVoteCount != number)
		{
			voteCountText.text = number.ToString();
			voteCountLabelText.text = ((number == 1) ? ScriptLocalization.Twitch_Voting.Num_Votes_Singular : ScriptLocalization.Twitch_Voting.Num_Votes);
			lastVoteCount = number;
		}
	}

	public void TriggerNewVoteAnimation()
	{
		if (animator.isInitialized)
		{
			animator.SetTrigger("NewVote");
		}
	}

	public void AnimateOffScreen()
	{
		StartCoroutine(AnimateOffScreenCoroutine());
	}

	public void AnimateToCenter()
	{
		StartCoroutine(AnimateToCenterCoroutine());
	}

	private IEnumerator AnimateToCenterCoroutine()
	{
		float t = 0f;
		Vector3 newPosition = default(Vector3);
		Vector3 start = animated.position;
		Vector3 end = VoteCanvas.transform.position;
		Quaternion startRotation = animated.transform.rotation;
		Quaternion EndRotation = Quaternion.Euler(0f, 0f, Random.Range(-45, 45));
		while (t <= animationTime)
		{
			newPosition.x = Mathf.Lerp(start.x, end.x, PartyBoxMoveX.Evaluate(t / animationTime));
			newPosition.y = Mathf.Lerp(start.y, end.y, PartyBoxMoveY.Evaluate(t / animationTime));
			Quaternion rotation = Quaternion.Lerp(startRotation, EndRotation, PartyBoxRotate.Evaluate(t / animationTime));
			animated.transform.position = newPosition;
			animated.transform.rotation = rotation;
			t += Time.unscaledDeltaTime;
			yield return null;
		}
		animated.transform.position = start;
		animated.transform.rotation = startRotation;
		ShowContents(show: false);
	}

	private IEnumerator AnimateOffScreenCoroutine()
	{
		float t = 0f;
		Vector3 newPosition = default(Vector3);
		Vector3 start = animated.position;
		Vector3 end = animated.position + new Vector3(0f, -100f);
		Quaternion startRotation = animated.transform.rotation;
		Quaternion EndRotation = Quaternion.Euler(0f, 0f, Random.Range(-45, 45));
		while (t <= animationTime)
		{
			newPosition.x = Mathf.Lerp(start.x, end.x, PartyBoxMoveX.Evaluate(t / animationTime));
			newPosition.y = Mathf.Lerp(start.y, end.y, PartyBoxMoveY.Evaluate(t / animationTime));
			Quaternion rotation = Quaternion.Lerp(startRotation, EndRotation, PartyBoxRotate.Evaluate(t / animationTime));
			animated.transform.position = newPosition;
			animated.transform.rotation = rotation;
			t += Time.unscaledDeltaTime;
			yield return null;
		}
		animated.transform.position = start;
		animated.transform.rotation = startRotation;
		ShowContents(show: false);
	}

	public void UpdateLocalizedItemName()
	{
		if (lastPickableIndex != -1)
		{
			string localizedShortName = TwitchTermMapper.GetLocalizedShortName(lastPickableIndex);
			if (localizedShortName != null)
			{
				voteItemName.text = localizedShortName;
			}
		}
	}
}
