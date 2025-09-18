using UnityEngine;

public class SlowMoImageEffect : MonoBehaviour
{
	public float FisheyeStrength;

	public float Saturation;

	public Material FisheyeMat;

	private float currFisheye;

	private float currSat;

	private void OnRenderImage(RenderTexture source, RenderTexture destination)
	{
		Graphics.Blit(source, destination, FisheyeMat);
	}

	private void Update()
	{
		float num = Time.timeScale;
		if (!Timekeeper.Slowing)
		{
			num = Modifiers.GetInstance().GameSpeed;
		}
		float num2 = num / Modifiers.GetInstance().GameSpeed;
		currFisheye = Mathf.Lerp(0f, 1f, 2f * (1f - num2)) * FisheyeStrength;
		currSat = 2f * (1f - Saturation) * num2 + 2f * Saturation - 1f;
		FisheyeMat.SetFloat("_Fisheye", currFisheye);
		FisheyeMat.SetFloat("_Saturation", currSat);
	}
}
