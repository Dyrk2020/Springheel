using UnityEngine;

public class CameraMoveAnimator : MonoBehaviour
{
	private Animator animator;

	public UIMenu startUi;

	public UIMenu mainMenuUi;

	public UIMenu optionsUi;

	public UIMenu creditsUI;

	private void Start()
	{
		animator = GetComponent<Animator>();
	}

	private void Update()
	{
		animator.SetBool("Start", startUi.Visible);
		animator.SetBool("Main", mainMenuUi.Visible);
		if (creditsUI.Visible | optionsUi.Visible)
		{
			animator.SetBool("Blank", value: true);
		}
		else
		{
			animator.SetBool("Blank", value: false);
		}
	}
}
