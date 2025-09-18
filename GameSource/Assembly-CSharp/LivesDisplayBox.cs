using System.Collections;
using System.Collections.Generic;
using I2.Loc;
using UnityEngine;
using UnityEngine.UI;

public class LivesDisplayBox : MonoBehaviour
{
	public List<Image> lifeSprites;

	public Text lifeText;

	public HoldBToGiveUp respawnButton;

	private int lastShownLives;

	public bool CanRespawn;

	private IEnumerator anim;

	public void Initialize(Character.Animals animal)
	{
		GameSettings.GetInstance();
		Sprite charaterPortrait = CharacterSpriteManager.GetInstance().GetCharaterPortrait(animal);
		for (int i = 0; i < lifeSprites.Count; i++)
		{
			lifeSprites[i].sprite = charaterPortrait;
		}
		respawnButton.text.text = LocalizationManager.GetTranslation("InGameText/Respawn");
		respawnButton.InstantHide();
	}

	public void SetNumLives(int lives, bool forceUpdate = false)
	{
		if (lastShownLives != lives || forceUpdate)
		{
			lastShownLives = lives;
			UpdateSprites();
		}
	}

	public void FillRespawnButton(float amount)
	{
		if (amount > 0f)
		{
			if (!respawnButton.Visible)
			{
				respawnButton.Show();
			}
			respawnButton.SetPulse(pulse: false);
		}
		else if (respawnButton.Visible)
		{
			respawnButton.SetPulse(pulse: true);
		}
		respawnButton.SetFillAmount(amount);
	}

	public void SetLocalController(Controller controller)
	{
		respawnButton.SetLocalController(controller);
	}

	private void UpdateSprites()
	{
		int count = lifeSprites.Count;
		if (lastShownLives <= count)
		{
			for (int i = 0; i < lifeSprites.Count; i++)
			{
				lifeSprites[i].gameObject.SetActive(i < lastShownLives);
			}
			lifeText.gameObject.SetActive(value: false);
			return;
		}
		for (int j = 0; j < lifeSprites.Count; j++)
		{
			lifeSprites[j].gameObject.SetActive(j == 0);
		}
		lifeText.gameObject.SetActive(value: true);
		lifeText.text = " x " + lastShownLives;
	}

	private void Update()
	{
		if (CanRespawn)
		{
			respawnButton.Show();
			if (anim != null)
			{
				StartWobble();
			}
		}
		else
		{
			respawnButton.Hide();
			if (anim != null)
			{
				StopWobble();
			}
		}
		if (anim != null && !anim.MoveNext())
		{
			anim = null;
		}
	}

	private void StartWobble()
	{
		anim = WobbleLifeImages();
	}

	private void StopWobble()
	{
		anim = null;
		for (int i = 0; i < lifeSprites.Count; i++)
		{
			lifeSprites[i].transform.localScale = Vector3.one;
		}
	}

	private IEnumerator WobbleLifeImages()
	{
		bool dir = true;
		float t = 0f;
		float cycle = 0.3f;
		float minScale = 0.8f;
		float maxScale = 1.2f;
		while (true)
		{
			float num;
			if (dir)
			{
				t += Time.deltaTime;
				num = Mathf.Lerp(maxScale, minScale, t / cycle);
				if (t >= cycle)
				{
					t = cycle;
					dir = !dir;
				}
			}
			else
			{
				t -= Time.deltaTime;
				num = Mathf.Lerp(maxScale, minScale, t / cycle);
				if (t < 0f)
				{
					t = 0f;
					dir = !dir;
				}
			}
			for (int i = 0; i < lifeSprites.Count; i++)
			{
				if (lifeSprites[i].gameObject.activeSelf)
				{
					lifeSprites[i].transform.localScale = new Vector3(num, num, num);
				}
			}
			yield return null;
		}
	}
}
