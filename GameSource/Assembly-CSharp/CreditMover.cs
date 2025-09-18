using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CreditMover : MonoBehaviour
{
	public Animator Animator;

	public bool MovementActivated;

	public LevelSelectController LevelSelectController;

	public Transform top;

	public Transform bottom;

	protected List<Text> AllCreditTexts = new List<Text>();

	protected List<Text> Remove = new List<Text>();

	protected List<Image> AllCreditImages = new List<Image>();

	protected List<Image> RemovedImages = new List<Image>();

	private void Start()
	{
		AllCreditTexts.AddRange(GetComponentsInChildren<Text>());
		AllCreditImages.AddRange(GetComponentsInChildren<Image>());
	}

	public void Go()
	{
		if (MovementActivated)
		{
			return;
		}
		MovementActivated = true;
		Animator.SetTrigger("StartCredits");
		AllCreditTexts.Clear();
		AllCreditTexts.AddRange(GetComponentsInChildren<Text>(includeInactive: true));
		AllCreditImages.Clear();
		AllCreditImages.AddRange(GetComponentsInChildren<Image>(includeInactive: true));
		foreach (Text allCreditText in AllCreditTexts)
		{
			allCreditText.enabled = false;
			allCreditText.GetComponent<Collider2D>().enabled = false;
			allCreditText.color = new Color(1f, 1f, 1f, 1f);
		}
		Remove.Clear();
		foreach (Image allCreditImage in AllCreditImages)
		{
			allCreditImage.enabled = false;
			allCreditImage.GetComponent<Collider2D>().enabled = false;
			allCreditImage.color = new Color(1f, 1f, 1f, 1f);
		}
		RemovedImages.Clear();
	}

	private void Update()
	{
		if (!MovementActivated)
		{
			return;
		}
		foreach (Text allCreditText in AllCreditTexts)
		{
			float y = allCreditText.transform.position.y;
			if (y > top.position.y)
			{
				StartCoroutine(FadeAndHideTxt(allCreditText));
				Remove.Add(allCreditText);
			}
			else if (y > bottom.position.y)
			{
				allCreditText.enabled = true;
				allCreditText.GetComponent<Collider2D>().enabled = true;
			}
		}
		foreach (Text item in Remove)
		{
			AllCreditTexts.Remove(item);
		}
		Remove.Clear();
		foreach (Image allCreditImage in AllCreditImages)
		{
			float y2 = allCreditImage.transform.position.y;
			if (y2 > top.position.y)
			{
				StartCoroutine(FadeAndHideImg(allCreditImage));
				RemovedImages.Add(allCreditImage);
			}
			else if (y2 > bottom.position.y)
			{
				allCreditImage.enabled = true;
				allCreditImage.GetComponent<Collider2D>().enabled = true;
			}
		}
		foreach (Image removedImage in RemovedImages)
		{
			AllCreditImages.Remove(removedImage);
		}
		RemovedImages.Clear();
	}

	private IEnumerator FadeAndHideTxt(Text txt)
	{
		float timer = 1f;
		while (timer > 0f)
		{
			txt.color = new Color(1f, 1f, 1f, timer);
			if (timer < 1f)
			{
				txt.GetComponent<Collider2D>().enabled = false;
			}
			timer -= Time.deltaTime;
			yield return null;
		}
		txt.enabled = false;
	}

	private IEnumerator FadeAndHideImg(Image img)
	{
		float timer = 1f;
		while (timer > 0f)
		{
			img.color = new Color(1f, 1f, 1f, timer);
			if (timer < 1f)
			{
				img.GetComponent<Collider2D>().enabled = false;
			}
			timer -= Time.deltaTime;
			yield return null;
		}
		img.enabled = false;
	}

	public void MovementDone()
	{
		MovementActivated = false;
	}

	public void CreditCameraBounds()
	{
		LevelSelectController.GotoCreditCameraMode();
	}

	public void ReturnToRegularCameraBounds()
	{
		LevelSelectController.GotoRegularCameraBounds();
	}
}
