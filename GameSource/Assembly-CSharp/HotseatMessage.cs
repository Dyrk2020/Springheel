using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class HotseatMessage : UIGraphic
{
	private enum HotseatController
	{
		KEYBOARD,
		CONTROLLER1,
		CONTROLLER2,
		CONTROLLER3,
		CONTROLLER4
	}

	public Image[] CharacterPortraits;

	public Image[] Pluses;

	public GameObject PS4ControllerImage;

	public GameObject Xb1ControllerImage;

	public GameObject SwitchControllerImage;

	public GameObject SwitchJoyconLImage;

	public GameObject SwitchJoyconRImage;

	public GameObject KeyboardControllerImage;

	public GameObject GenericControllerImage;

	public Text InstructionText;

	public Text PickLevelText;

	public Text SharingText;

	public Image background;

	public Sprite[] CharacterPortraitSprites;

	public Sprite[] Instructions;

	private float showTime;

	private float showTimer;

	private CanvasGroup canvasgroup;

	public float fadeTime;

	private Coroutine fade;

	private void Start()
	{
		Hide();
		canvasgroup = GetComponent<CanvasGroup>();
	}

	public override void Update()
	{
		if (showTime > 0f)
		{
			showTimer += Time.unscaledDeltaTime;
			if (showTimer >= showTime + fadeTime)
			{
				StartCoroutine(Fade(1f, 0f, fadeTime, hide: true));
				showTime = 0f;
				showTimer = 0f;
			}
		}
	}

	public void ShowMessage(Player addedPlayer, Player[] previousPlayers, float time, bool canChooseAnother)
	{
		if (previousPlayers.Length >= 1)
		{
			Hide();
			int num = 0;
			int num2 = 0;
			if (previousPlayers.Length == 1)
			{
				num = 1;
				num2 = 1;
			}
			else if (previousPlayers.Length == 2)
			{
				num = 4;
				num2 = 3;
			}
			for (int i = 0; i != previousPlayers.Length; i++)
			{
				CharacterPortraits[i + num].enabled = true;
				CharacterPortraits[i + num].sprite = CharacterSpriteManager.GetInstance().GetCharaterPortrait(previousPlayers[i].PlayerCharacter.CharacterSprite);
				Pluses[i + num2].enabled = true;
			}
			CharacterPortraits[previousPlayers.Length + num].enabled = true;
			CharacterPortraits[previousPlayers.Length + num].sprite = CharacterSpriteManager.GetInstance().GetCharaterPortrait(addedPlayer.PlayerCharacter.CharacterSprite);
			Controller useController = addedPlayer.UseController;
			SetControllerImage(useController);
			SharingText.enabled = true;
			if (canChooseAnother)
			{
				InstructionText.enabled = true;
				PickLevelText.enabled = false;
			}
			else
			{
				InstructionText.enabled = false;
				PickLevelText.enabled = true;
			}
			background.enabled = true;
			if (fade != null)
			{
				StopCoroutine(fade);
			}
			fade = StartCoroutine(Fade(0f, 1f, fadeTime, hide: false));
			showTimer = 0f;
			showTime = time;
		}
	}

	public override void Hide(bool forceQuickHide = false)
	{
		base.Hide(forceQuickHide);
	}

	public void SetControllerImage(Controller c)
	{
		HideControllerImage();
		GameObject gameObject = MultiControllerUIManager.GetControllerType(c) switch
		{
			MultiControllerUIManager.ControllerType.KeyboardAndMouse => KeyboardControllerImage, 
			MultiControllerUIManager.ControllerType.KeyboardAndMouseAlt => KeyboardControllerImage, 
			MultiControllerUIManager.ControllerType.Xbox => Xb1ControllerImage, 
			MultiControllerUIManager.ControllerType.DualShock4 => PS4ControllerImage, 
			MultiControllerUIManager.ControllerType.SingleJoyconL => SwitchJoyconLImage, 
			MultiControllerUIManager.ControllerType.SingleJoyconR => SwitchJoyconRImage, 
			MultiControllerUIManager.ControllerType.Switch => SwitchControllerImage, 
			_ => GenericControllerImage, 
		};
		gameObject.SetActive(value: true);
		Image[] componentsInChildren = gameObject.GetComponentsInChildren<Image>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].enabled = true;
		}
	}

	public void HideControllerImage()
	{
		PS4ControllerImage.SetActive(value: false);
		Xb1ControllerImage.SetActive(value: false);
		SwitchControllerImage.SetActive(value: false);
		KeyboardControllerImage.SetActive(value: false);
		GenericControllerImage.SetActive(value: false);
	}

	public override void Show()
	{
	}

	private IEnumerator Fade(float startAlpha, float endAlpha, float duration, bool hide)
	{
		float timer = 0f;
		while (timer < duration)
		{
			canvasgroup.alpha = Mathf.Lerp(startAlpha, endAlpha, timer / duration);
			timer += Time.unscaledDeltaTime;
			yield return null;
		}
		canvasgroup.alpha = endAlpha;
		if (hide)
		{
			Hide();
		}
	}
}
