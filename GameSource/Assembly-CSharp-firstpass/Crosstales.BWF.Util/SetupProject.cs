using Crosstales.BWF.Manager;
using Crosstales.Common.Util;
using UnityEngine;

namespace Crosstales.BWF.Util;

public class SetupProject
{
	static SetupProject()
	{
		setup();
	}

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
	private static void setup()
	{
		Singleton<BWFManager>.PrefabPath = "Prefabs/BWF";
		Singleton<BWFManager>.GameObjectName = "BWF";
		Singleton<BadWordManager>.PrefabPath = "Prefabs/BadWordManager";
		Singleton<BadWordManager>.GameObjectName = "BadWordManager";
		Singleton<CapitalizationManager>.PrefabPath = "Prefabs/CapitalizationManager";
		Singleton<CapitalizationManager>.GameObjectName = "CapitalizationManager";
		Singleton<DomainManager>.PrefabPath = "Prefabs/DomainManager";
		Singleton<DomainManager>.GameObjectName = "DomainManager";
		Singleton<PunctuationManager>.PrefabPath = "Prefabs/PunctuationManager";
		Singleton<PunctuationManager>.GameObjectName = "PunctuationManager";
	}
}
