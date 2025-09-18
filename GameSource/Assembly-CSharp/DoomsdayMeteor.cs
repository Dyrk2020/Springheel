using UnityEngine;

public class DoomsdayMeteor : MonoBehaviour
{
	public enum MeteorSizes
	{
		ExtraSmall = 1,
		Small,
		Medium,
		Large,
		ExtraLarge
	}

	public float speed = 10f;

	public float yMin;

	public MeteorSizes size;

	private void Start()
	{
	}

	public void StartMeteorSound()
	{
		AkSoundEngine.PostEvent("ENV_DoomsdayMeteor_In_" + GetMeteorSizeString(), base.gameObject);
	}

	public void OnScreenMeteorSound()
	{
		AkSoundEngine.PostEvent("UCH_ENV_Doomsday_Meteor_" + GetMeteorSizeString() + "_OnScreen", base.gameObject);
	}

	private void FixedUpdate()
	{
		if (!GameState.GetInstance().Paused)
		{
			base.transform.position += base.transform.up * speed * Time.deltaTime;
			if (base.transform.position.y < yMin)
			{
				Object.Destroy(base.gameObject);
			}
		}
	}

	private void OnDestroy()
	{
		AkSoundEngine.PostEvent("ENV_DoomsdayMeteor_Out_" + GetMeteorSizeString(), base.gameObject);
	}

	private string GetMeteorSizeString()
	{
		return size switch
		{
			MeteorSizes.ExtraSmall => "ExtraSmall", 
			MeteorSizes.Small => "Small", 
			MeteorSizes.Medium => "Medium", 
			MeteorSizes.Large => "Large", 
			MeteorSizes.ExtraLarge => "ExtraLarge", 
			_ => "Medium", 
		};
	}
}
