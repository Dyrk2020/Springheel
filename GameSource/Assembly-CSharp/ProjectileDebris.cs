using UnityEngine;

public class ProjectileDebris : MonoBehaviour
{
	public Transform ExplosionParticle;

	public float particleScaleFactor = 1f;

	public void Awake()
	{
		Modifiers instance = Modifiers.GetInstance();
		if (instance.ProjectilesExplode && ExplosionParticle != null)
		{
			ExplosionParticle.gameObject.SetActive(value: true);
			ExplosionParticle.SetParent(null, worldPositionStays: true);
			float self = instance.ProjectileExplosionScale * particleScaleFactor;
			ExplosionParticle.localScale = self.MakeVector3();
			if (instance.ProjectileExplosionMode == 4)
			{
				AkSoundEngine.PostEvent("SFX_Projectile_Explode_Huge", base.gameObject);
			}
			else if (instance.ProjectileExplosionMode == 3)
			{
				AkSoundEngine.PostEvent("SFX_Projectile_Explode_Big", base.gameObject);
			}
			else if (instance.ProjectileExplosionMode == 2)
			{
				AkSoundEngine.PostEvent("SFX_Projectile_Explode_Medium", base.gameObject);
			}
			else if (instance.ProjectileExplosionMode == 1)
			{
				AkSoundEngine.PostEvent("SFX_Projectile_Explode_Small", base.gameObject);
			}
			else
			{
				AkSoundEngine.PostEvent("SFX_Projectile_Explode_Small", base.gameObject);
			}
		}
	}
}
