using UnityEngine;
using UnityEngine.UI;

public class OutfitRowController : MonoBehaviour
{
	public SpriteRenderer OutfitImage;

	public SpriteRenderer leftArrow;

	public SpriteRenderer RightArrow;

	protected Animator animator;

	public Text amountText;

	public Outfit.OutfitType OutfitType;

	private void Awake()
	{
		animator = GetComponent<Animator>();
	}

	private void Start()
	{
		Hide();
	}

	public void setOutfitImage(Outfit outfit)
	{
		if (outfit != null)
		{
			if (OutfitType == Outfit.OutfitType.Skin)
			{
				OutfitImage.sprite = null;
				return;
			}
			OutfitImage.sprite = outfit.UISprite;
			OutfitImage.transform.localPosition = new Vector3(outfit.UISpriteOffset.x, outfit.UISpriteOffset.y, 0f);
			OutfitImage.transform.localScale = Vector3.one * outfit.UISpriteScale;
			OutfitImage.color = Color.white;
			MaterialPropertyBlock materialPropertyBlock = new MaterialPropertyBlock();
			OutfitImage.GetPropertyBlock(materialPropertyBlock);
			materialPropertyBlock.SetFloat("_HueShiftAmount", outfit.hueShift);
			materialPropertyBlock.SetFloat("_SatShiftAmount", outfit.saturationShift);
			materialPropertyBlock.SetFloat("_ValShiftAmount", outfit.valueShift);
			materialPropertyBlock.SetFloat("_ContrastShiftAmount", outfit.contrastShift);
			materialPropertyBlock.SetFloat("_Colorize", (!outfit.colorize) ? 1 : 0);
			OutfitImage.SetPropertyBlock(materialPropertyBlock);
		}
	}

	public void setOutfitImage(Sprite outfitSprite, Color color, float scale, Vector3 hsvShift)
	{
		if (outfitSprite != null && OutfitType != Outfit.OutfitType.Skin)
		{
			OutfitImage.sprite = outfitSprite;
			OutfitImage.transform.localPosition = Vector3.zero;
			OutfitImage.transform.localScale = Vector3.one * scale;
			OutfitImage.color = color;
			MaterialPropertyBlock materialPropertyBlock = new MaterialPropertyBlock();
			OutfitImage.GetPropertyBlock(materialPropertyBlock);
			materialPropertyBlock.SetFloat("_HueShiftAmount", hsvShift.x);
			materialPropertyBlock.SetFloat("_SatShiftAmount", hsvShift.y);
			materialPropertyBlock.SetFloat("_ValShiftAmount", hsvShift.z);
			OutfitImage.SetPropertyBlock(materialPropertyBlock);
		}
	}

	public void Show()
	{
		OutfitImage.enabled = true;
		leftArrow.enabled = true;
		RightArrow.enabled = true;
		amountText.enabled = true;
	}

	public void Hide()
	{
		OutfitImage.enabled = false;
		leftArrow.enabled = false;
		RightArrow.enabled = false;
		amountText.enabled = false;
	}

	public void Select(bool selected)
	{
		animator.SetBool("Selected", selected);
	}

	public void Right()
	{
		animator.SetTrigger("GoRight");
		AkSoundEngine.PostEvent("UI_Lobby_Arrow_Selection", base.gameObject);
	}

	public void Left()
	{
		animator.SetTrigger("GoLeft");
		AkSoundEngine.PostEvent("UI_Lobby_Arrow_Selection", base.gameObject);
	}

	public void setText(string amount)
	{
		amountText.text = amount;
	}
}
