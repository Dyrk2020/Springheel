using UnityEngine;

public class TreeHouseRenderControl : MonoBehaviour
{
	public Camera overlayCamera;

	public UndergroundTrigger GroundHider;

	public PartyModeToggle ColourReference;

	public Shader TransparencyShader;

	private Material mat;

	private bool overlayEnabled;

	private void Awake()
	{
		mat = new Material(TransparencyShader);
	}

	public void EnableOverlay()
	{
		overlayEnabled = true;
	}

	private void Start()
	{
		overlayCamera.enabled = false;
	}

	private void Update()
	{
		if (overlayEnabled)
		{
			Color currentColor = ColourReference.cs[2].currentColor;
			currentColor.a = 0f;
			overlayCamera.clearFlags = CameraClearFlags.Color;
			overlayCamera.backgroundColor = currentColor;
		}
	}

	private void OnPostRender()
	{
		if (overlayEnabled)
		{
			RenderTexture temporary = RenderTexture.GetTemporary(Screen.width, Screen.height, 0, RenderTextureFormat.ARGB32);
			overlayCamera.targetTexture = temporary;
			overlayCamera.Render();
			overlayCamera.targetTexture = null;
			mat.SetFloat("_Alpha", GroundHider.alpha);
			Graphics.Blit(temporary, null, mat);
			RenderTexture.ReleaseTemporary(temporary);
		}
	}
}
