using GameEvent;
using I2.Loc;
using UnityEngine.UI;

public class PickableButtonLanguages : PickableButton
{
	public enum ButtonLanguages
	{
		en,
		it,
		de,
		zhCN,
		frFR,
		ja,
		es,
		ru,
		ptBR,
		ptPT,
		zhTW,
		ko,
		pl,
		tr,
		sv,
		fi,
		th,
		vi,
		updateLanguages
	}

	public ButtonLanguages job;

	protected override void Awake()
	{
		if (buttonText == null)
		{
			buttonText = GetComponent<Text>();
		}
		base.Awake();
	}

	public override void OnAccept(PickCursor pickCursor)
	{
		base.OnAccept(pickCursor);
		string currentLanguage = LocalizationManager.CurrentLanguage;
		switch (job)
		{
		case ButtonLanguages.en:
			LocalizationManager.CurrentLanguage = "English";
			break;
		case ButtonLanguages.it:
			LocalizationManager.CurrentLanguage = "Italian";
			break;
		case ButtonLanguages.de:
			LocalizationManager.CurrentLanguage = "German";
			break;
		case ButtonLanguages.zhCN:
			LocalizationManager.CurrentLanguage = "Chinese (Simplified)";
			break;
		case ButtonLanguages.frFR:
			LocalizationManager.CurrentLanguage = "French (France)";
			break;
		case ButtonLanguages.ja:
			LocalizationManager.CurrentLanguage = "Japanese";
			break;
		case ButtonLanguages.es:
			LocalizationManager.CurrentLanguage = "Spanish (Spain)";
			break;
		case ButtonLanguages.ru:
			LocalizationManager.CurrentLanguage = "Russian";
			break;
		case ButtonLanguages.ptBR:
			LocalizationManager.CurrentLanguage = "Portuguese (Brazil)";
			break;
		case ButtonLanguages.ptPT:
			LocalizationManager.CurrentLanguage = "Portuguese (Portugal)";
			break;
		case ButtonLanguages.zhTW:
			LocalizationManager.CurrentLanguage = "Chinese (Traditional)";
			break;
		case ButtonLanguages.ko:
			LocalizationManager.CurrentLanguage = "Korean";
			break;
		case ButtonLanguages.pl:
			LocalizationManager.CurrentLanguage = "polish";
			break;
		case ButtonLanguages.tr:
			LocalizationManager.CurrentLanguage = "Turkish";
			break;
		case ButtonLanguages.sv:
			LocalizationManager.CurrentLanguage = "Swedish";
			break;
		case ButtonLanguages.fi:
			LocalizationManager.CurrentLanguage = "finnish";
			break;
		case ButtonLanguages.th:
			LocalizationManager.CurrentLanguage = "thai";
			break;
		case ButtonLanguages.vi:
			LocalizationManager.CurrentLanguage = "vie";
			break;
		case ButtonLanguages.updateLanguages:
			LocalizationManager.Sources[0].Import_Google(ForceUpdate: true, justCheck: false);
			break;
		}
		if (currentLanguage != LocalizationManager.CurrentLanguage)
		{
			GameEventManager.SendEvent(new LanguageChangeEvent(LocalizationManager.CurrentLanguage));
		}
	}
}
