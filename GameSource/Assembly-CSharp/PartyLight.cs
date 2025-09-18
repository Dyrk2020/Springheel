using System;
using UnityEngine;

[Serializable]
public class PartyLight
{
	public Light light;

	public float targetLightIntensity;

	public float initialLightIntensity;

	public float currentLightIntensity;

	public Color targetLightColor;

	public Color initialLightColor;

	public Color currentLightColor;

	public float targetlightCookieSize;

	public float initialLightCookieSize;

	public float currentLightCookieSize;

	public float changeTime = 0.5f;

	protected bool coroutineLock;

	public void setInitialLight()
	{
		initialLightColor = light.color;
		initialLightIntensity = light.intensity;
		initialLightCookieSize = light.cookieSize;
	}

	public void setLight(float t)
	{
		currentLightColor = initialLightColor;
		currentLightIntensity = initialLightIntensity;
		currentLightCookieSize = initialLightCookieSize;
		currentLightColor = Color.Lerp(initialLightColor, targetLightColor, t);
		currentLightIntensity = Mathf.Lerp(initialLightIntensity, targetLightIntensity, t);
		currentLightCookieSize = Mathf.Lerp(initialLightCookieSize, targetlightCookieSize, t);
		light.color = currentLightColor;
		light.intensity = currentLightIntensity;
		light.cookieSize = currentLightCookieSize;
	}
}
