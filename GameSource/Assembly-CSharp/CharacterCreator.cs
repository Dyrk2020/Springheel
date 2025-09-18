using System;
using UnityEngine;

[Serializable]
public class CharacterCreator : ScriptableObject
{
	[Serializable]
	public struct AnimationPiece
	{
		public string animationName;

		public string baseAnimatorName;

		public float FrameRate;

		public Sprite[] animationFrames;

		public AnimationClip referenceAnimation;
	}

	public string internalCharacterName;

	public Character.Animals animal;

	public Sprite OKCursor;

	public Sprite BadCursor;

	public Sprite NotebookCursor;

	public Sprite Portrait;

	public Sprite AliveOutline;

	public Sprite DeadOutline;

	public AnimatorOverrideController CharacterSpriteOverride;

	public AnimatorOverrideController SpectatorSpriteOverride;

	public Color zombieColor;

	public AnimationPiece[] animationPieces;

	public string ReferenceCharacterName;

	public AnimatorOverrideController ReferenceOverrideController;

	public bool showIndividualAnimationUpdater;
}
