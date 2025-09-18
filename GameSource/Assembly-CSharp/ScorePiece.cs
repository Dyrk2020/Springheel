using UnityEngine;
using UnityEngine.UI;

public class ScorePiece : MonoBehaviour
{
	public Image pieceImage;

	public float width;

	private Animator animator;

	public Text text;

	public PointBlock.pointBlockType pointBlockType;

	public ScorePiece suicideblock;

	public string SFXEventName;

	public float textPadding = 5f;

	private void Awake()
	{
		pieceImage = GetComponent<Image>();
		animator = GetComponent<Animator>();
		text = GetComponentInChildren<Text>();
		text.color = pieceImage.color;
		width = GameSettings.GetInstance().PointTypeValue(pointBlockType);
	}

	public void animate()
	{
		animator.SetTrigger("TurnOn");
		AkSoundEngine.PostEvent(SFXEventName, base.gameObject);
	}

	public void destroyScorePiece()
	{
		Object.Destroy(suicideblock.gameObject);
		Object.Destroy(base.gameObject);
	}

	public void setImageWidth(float sizeFactor)
	{
		float num = Mathf.Abs(width) * sizeFactor;
		pieceImage.rectTransform.sizeDelta = new Vector2(num, pieceImage.rectTransform.sizeDelta.y);
		text.rectTransform.localPosition = new Vector3(num + textPadding, text.rectTransform.localPosition.y, text.rectTransform.localPosition.z);
	}
}
